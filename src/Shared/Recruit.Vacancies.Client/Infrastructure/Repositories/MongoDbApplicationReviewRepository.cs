using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using MongoDB.Bson;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    internal sealed class MongoDbApplicationReviewRepository : MongoDbCollectionBase, IApplicationReviewRepository
    {
        private const string Database = "recruit";
        private const string Collection = "applicationReviews";
        private const string EmployerAccountId = "employerAccountId";
        private const string VacancyReference = "vacancyReference";
        private const string Id = "_id";
        

        public MongoDbApplicationReviewRepository(ILogger<MongoDbApplicationReviewRepository> logger, IOptions<MongoDbConnectionDetails> details)
            : base(logger, Database, Collection, details)
        {
        }

        public Task CreateAsync(ApplicationReview review)
        {
            var collection = GetCollection<ApplicationReview>();
            return RetryPolicy.ExecuteAsync(context => collection.InsertOneAsync(review), new Context(nameof(CreateAsync)));
        }

        public async Task<List<T>> GetApplicationReviewsForEmployerAsync<T>(string employerAccountId)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(EmployerAccountId, employerAccountId);
            return await QueryApplicationReviews<T>(filter);
        }

        public async Task<List<T>> GetApplicationReviewsForVacancyAsync<T>(long vacancyReference)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(VacancyReference, vacancyReference);
            return await QueryApplicationReviews<T>(filter);
        }

        public async Task<T> GetApplicationReviewAsync<T>(Guid applicationReviewId)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(Id, applicationReviewId);
            var applicationReview = await QueryApplicationReviews<T>(filter);
            return applicationReview.SingleOrDefault();
        }

        private async Task<List<T>> QueryApplicationReviews<T>(FilterDefinition<BsonDocument> filter)
        {
            var collection = GetCollection<BsonDocument>();

            var result = await RetryPolicy.ExecuteAsync(context => collection.FindAsync<T>(filter),
                new Context(nameof(GetApplicationReviewsForEmployerAsync)));

            return await result.ToListAsync();
        }
    }
}
