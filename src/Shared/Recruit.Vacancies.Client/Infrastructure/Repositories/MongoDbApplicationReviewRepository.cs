using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using System.Linq;
using MongoDB.Bson;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    internal sealed class MongoDbApplicationReviewRepository : MongoDbCollectionBase, IApplicationReviewRepository
    {
        private const string Database = "recruit";
        private const string Collection = "applicationReviews";
        private const string EmployerAccountId = "employerAccountId";

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
            var collection = GetCollection<BsonDocument>();

            var result = await RetryPolicy.ExecuteAsync(context => collection.FindAsync<T>(filter),
                new Context(nameof(GetApplicationReviewsForEmployerAsync)));

            return await result.ToListAsync();
        }
    }
}
