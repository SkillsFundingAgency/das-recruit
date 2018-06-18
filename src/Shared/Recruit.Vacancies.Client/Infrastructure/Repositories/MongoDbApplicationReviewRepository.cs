using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    internal sealed class MongoDbApplicationReviewRepository : MongoDbCollectionBase, IApplicationReviewRepository
    {
        private const string Database = "recruit";
        private const string Collection = "applicationReviews";

        public MongoDbApplicationReviewRepository(ILogger<MongoDbApplicationReviewRepository> logger, IOptions<MongoDbConnectionDetails> details)
            : base(logger, Database, Collection, details)
        {
        }

        public Task CreateAsync(ApplicationReview review)
        {
            var collection = GetCollection<ApplicationReview>();
            return RetryPolicy.ExecuteAsync(context => collection.InsertOneAsync(review), new Context(nameof(CreateAsync)));
        }
    }
}
