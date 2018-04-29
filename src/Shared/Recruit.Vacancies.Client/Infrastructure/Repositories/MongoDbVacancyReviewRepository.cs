using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    internal sealed class MongoDbVacancyReviewRepository : MongoDbCollectionBase, IVacancyReviewRepository
    {
        private const string Database = "recruit";
        private const string Collection = "vacancyReviews";

        public MongoDbVacancyReviewRepository(IOptions<MongoDbConnectionDetails> details) 
            : base(Database, Collection, details)
        {
        }

        public async Task CreateAsync(VacancyReview vacancy)
        {
            var collection = GetCollection<VacancyReview>();
            await collection.InsertOneAsync(vacancy);
        }

        public async Task<VacancyReview> GetAsync(Guid reviewId)
        {
            var filter = Builders<VacancyReview>.Filter.Eq(r => r.Id, reviewId);

            var collection = GetCollection<VacancyReview>();
            var result = await collection.FindAsync(filter);
            return result.SingleOrDefault();
        }

        public async Task<IEnumerable<VacancyReview>> GetAllAsync()
        {
            var collection = GetCollection<VacancyReview>();
            var result = await collection
                                    .Find(FilterDefinition<VacancyReview>.Empty)
                                    .Sort(Builders<VacancyReview>.Sort.Descending(r => r.CreatedDate))
                                    .ToListAsync();

            return result;
        }

        public Task UpdateAsync(VacancyReview review)
        {
            var filter = Builders<VacancyReview>.Filter.Eq(v => v.Id, review.Id);
            var collection = GetCollection<VacancyReview>();
           
            return collection.ReplaceOneAsync(filter, review);
        }
    }
}
