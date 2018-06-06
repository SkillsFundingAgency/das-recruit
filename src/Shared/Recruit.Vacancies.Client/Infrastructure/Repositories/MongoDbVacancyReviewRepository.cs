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

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    internal sealed class MongoDbVacancyReviewRepository : MongoDbCollectionBase, IVacancyReviewRepository
    {
        private const string Database = "recruit";
        private const string Collection = "vacancyReviews";

        public MongoDbVacancyReviewRepository(ILogger<MongoDbVacancyReviewRepository> logger, IOptions<MongoDbConnectionDetails> details) 
            : base(logger, Database, Collection, details)
        {
        }

        public Task CreateAsync(VacancyReview vacancy)
        {
            var collection = GetCollection<VacancyReview>();
            return RetryPolicy.ExecuteAsync(() => collection.InsertOneAsync(vacancy));
        }

        public async Task<VacancyReview> GetAsync(Guid reviewId)
        {
            var filter = Builders<VacancyReview>.Filter.Eq(r => r.Id, reviewId);

            var collection = GetCollection<VacancyReview>();
            var result = await RetryPolicy.ExecuteAsync(() => collection.FindAsync(filter));
            return result.SingleOrDefault();
        }

        public async Task<IEnumerable<VacancyReview>> GetAllAsync()
        {
            var collection = GetCollection<VacancyReview>();
            var result = await RetryPolicy.ExecuteAsync(() => collection
                                    .Find(FilterDefinition<VacancyReview>.Empty)
                                    .Sort(Builders<VacancyReview>.Sort.Descending(r => r.CreatedDate))
                                    .ToListAsync());

            return result;
        }

        public Task UpdateAsync(VacancyReview review)
        {
            var filterBuilder = Builders<VacancyReview>.Filter;
            var filter = filterBuilder.Eq(v => v.Id, review.Id) & filterBuilder.Eq(v => v.VacancyReference, review.VacancyReference);
            var collection = GetCollection<VacancyReview>();
           
            return RetryPolicy.ExecuteAsync(() => collection.ReplaceOneAsync(filter, review));
        }
    }
}
