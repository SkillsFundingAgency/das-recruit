using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;

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

        public async Task<List<QaVacancySummary>> SearchAsync(long vacancyReference)
        {
            var filterBuilder = Builders<VacancyReview>.Filter;

            var filter = (filterBuilder.Eq(r => r.Status, ReviewStatus.PendingReview)
                         | filterBuilder.Eq(r => r.Status, ReviewStatus.UnderReview)) 
                         & filterBuilder.Eq(r => r.VacancyReference, vacancyReference);
            var collection = GetCollection<VacancyReview>();
            
            var result = await RetryPolicy
                .ExecuteAsync( 
                    context => collection
                        .Find(filter)
                        .Project(r => new QaVacancySummary()
                        {
                            Id = r.Id,
                            Title = r.Title,
                            VacancyReference = r.VacancyReference,
                            ReviewAssignedToUserName = r.ReviewedByUser.Name,
                            ReviewAssignedToUserId = r.ReviewedByUser.UserId,
                            ReviewStartedOn = r.ReviewedDate,
                            EmployerName = r.VacancySnapshot.EmployerName,
                            ClosingDate = r.VacancySnapshot.ClosingDate.Value,
                            SubmittedDate = r.VacancySnapshot.SubmittedDate.Value
                        })
                        .ToListAsync(),
                    new Context(nameof(SearchAsync)))
                .ConfigureAwait(false);

            return result;
        }

        public Task CreateAsync(VacancyReview vacancy)
        {
            var collection = GetCollection<VacancyReview>();
            return RetryPolicy.ExecuteAsync(context => collection.InsertOneAsync(vacancy), new Context(nameof(CreateAsync)));
        }

        public async Task<VacancyReview> GetAsync(Guid reviewId)
        {
            var filter = Builders<VacancyReview>.Filter.Eq(r => r.Id, reviewId);

            var collection = GetCollection<VacancyReview>();
            var result = await RetryPolicy.ExecuteAsync(context => collection.FindAsync(filter), new Context(nameof(GetAsync)));
            return result.SingleOrDefault();
        }

        public async Task<List<VacancyReview>> GetActiveAsync()
        {
            var filterBuilder = Builders<VacancyReview>.Filter;

            var filter = filterBuilder.Eq(r => r.Status, ReviewStatus.PendingReview)
                         | filterBuilder.Eq(r => r.Status, ReviewStatus.UnderReview);

            var collection = GetCollection<VacancyReview>();
            var result = await RetryPolicy.ExecuteAsync(context => collection
                                    .Find(filter)
                                    .ToListAsync(), new Context(nameof(GetActiveAsync)));

            return result.OrderByDescending(x => x.CreatedDate).ToList();
        }

        public Task UpdateAsync(VacancyReview review)
        {
            var filterBuilder = Builders<VacancyReview>.Filter;
            var filter = filterBuilder.Eq(v => v.Id, review.Id) & filterBuilder.Eq(v => v.VacancyReference, review.VacancyReference);
            var collection = GetCollection<VacancyReview>();
           
            return RetryPolicy.ExecuteAsync(context => collection.ReplaceOneAsync(filter, review), new Context(nameof(UpdateAsync)));
        }

        public Task<List<VacancyReview>> GetForVacancyAsync(long vacancyReference)
        {
            var filter = Builders<VacancyReview>.Filter.Eq(r => r.VacancyReference, vacancyReference);

            var collection = GetCollection<VacancyReview>();
            return RetryPolicy.ExecuteAsync(context => collection
                .Find(filter)
                .ToListAsync(), new Context(nameof(GetForVacancyAsync)));
        }
    }
}
