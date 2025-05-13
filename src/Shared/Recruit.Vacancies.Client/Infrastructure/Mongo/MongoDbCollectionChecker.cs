using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo
{
    public class MongoDbCollectionChecker
    {
        private readonly ILogger<MongoDbCollectionChecker> _logger;
        private readonly MongoDbConnectionDetails _config;

        public MongoDbCollectionChecker(ILogger<MongoDbCollectionChecker> logger, IOptions<MongoDbConnectionDetails> config)
        {
            _logger = logger;
            _config = config.Value;
        }

        public void EnsureCollectionsExist()
        {
            _logger.LogInformation("Ensuring collections have been created");

            var expectedCollections = GetExpectedCollectionNames();
            if(expectedCollections.Any() == false)
                throw new InfrastructureException("There are no expected collections.");

            var actualCollections = GetMongoCollectionsAsync(_logger).Result;

            var missingCollections = expectedCollections.Except(actualCollections).ToList();

            if(missingCollections.Any())
                throw new InfrastructureException($"Expected that collection(s): '{string.Join(", ", missingCollections)}' would already be created.");
        }

        public async Task CreateIndexes()
        {
            var db = GetMongoDatabase();
            await db.GetCollection<Vacancy>(MongoDbCollectionNames.Vacancies).Indexes.CreateManyAsync(new []
                {
                    new CreateIndexModel<Vacancy>(
                        Builders<Vacancy>.IndexKeys
                            .Ascending(d => d.TrainingProvider.Ukprn)
                            .Ascending(d => d.OwnerType)
                            .Ascending(d => d.Status)
                            .Ascending(d => d.IsDeleted)
                        ),
                    new CreateIndexModel<Vacancy>(
                        Builders<Vacancy>.IndexKeys
                            .Ascending(d => d.EmployerAccountId)
                            .Ascending(d => d.OwnerType)
                            .Ascending(d => d.Status)
                            .Ascending(d => d.IsDeleted)
                    ),
                    new CreateIndexModel<Vacancy>(
                        Builders<Vacancy>.IndexKeys
                            .Descending(d => d.CreatedDate)
                    ),
                    new CreateIndexModel<Vacancy>(
                        Builders<Vacancy>.IndexKeys
                            .Descending(d => d.CreatedDate)
                            .Ascending(d => d.EmployerAccountId)
                    ),
                    new CreateIndexModel<Vacancy>(
                        Builders<Vacancy>.IndexKeys
                            .Descending(d => d.CreatedDate)
                            .Ascending(d => d.TrainingProvider.Ukprn)
                    )
                }, new CreateManyIndexesOptions
            {
                MaxTime = TimeSpan.FromHours(1)
            });
            await db.GetCollection<ApplicationReview>(MongoDbCollectionNames.ApplicationReviews).Indexes.CreateManyAsync(new []
            {
                new CreateIndexModel<ApplicationReview>(Builders<ApplicationReview>.IndexKeys.Ascending(d => d.VacancyReference)),
                new CreateIndexModel<ApplicationReview>(Builders<ApplicationReview>.IndexKeys.Ascending(d => d.VacancyReference).Ascending(d=>d.IsWithdrawn)),
                new CreateIndexModel<ApplicationReview>(Builders<ApplicationReview>.IndexKeys.Ascending(d => d.Status)),
                new CreateIndexModel<ApplicationReview>(Builders<ApplicationReview>.IndexKeys.Ascending(d => d.IsWithdrawn)),
                new CreateIndexModel<ApplicationReview>(Builders<ApplicationReview>.IndexKeys.Ascending(d => d.IsWithdrawn).Ascending(d=>d.Status))
            }, new CreateManyIndexesOptions
            {
                MaxTime = TimeSpan.FromHours(1)
            });
            
        }

        private List<string> GetExpectedCollectionNames()
        {
            var consts = typeof(MongoDbCollectionNames)
                .GetFields(BindingFlags.Static | BindingFlags.NonPublic).ToList();

            return consts.Select(c => ((string)c.GetValue(null)).ToLower()).ToList();
        }

        private IMongoDatabase GetMongoDatabase()
        {
            var settings = MongoClientSettings.FromUrl(new MongoUrl(_config.ConnectionString));
            settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };
            var client = new MongoClient(settings);
            return client.GetDatabase(MongoDbNames.RecruitDb);
        }

        private async Task<List<string>> GetMongoCollectionsAsync(ILogger logger)
        {
            var db = GetMongoDatabase();

            var collections = await MongoDbRetryPolicy.GetRetryPolicy(logger).ExecuteAsync(context =>
                    db.ListCollectionNames().ToListAsync()
                , new Context(nameof(GetMongoCollectionsAsync)));

            return collections.Select(c => c.ToLower()).ToList();
        }
    }
}
