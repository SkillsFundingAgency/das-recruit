using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo
{
    public class MongoDbCollectionChecker
    {
        private readonly MongoDbConnectionDetails _config;

        public MongoDbCollectionChecker(IOptions<MongoDbConnectionDetails> config)
        {
            _config = config.Value;
        }

        public void EnsureCollectionsExist(ILogger logger)
        {
            var expectedCollections = GetExpectedCollectionNames();
            if(expectedCollections.Any() == false)
                throw new InfrastructureException("There are no expected collections.");

            var actualCollections = GetMongoCollectionsAsync(logger).Result;

            var missingCollections = expectedCollections.Except(actualCollections).ToList();

            if(missingCollections.Any())
                throw new InfrastructureException($"Expected that collection(s): '{string.Join(", ", missingCollections)}' would already be created.");
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
