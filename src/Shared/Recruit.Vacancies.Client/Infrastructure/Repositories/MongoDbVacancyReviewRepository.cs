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

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    internal sealed class MongoDbVacancyReviewRepository : MongoDbCollectionBase, IVacancyReviewRepository
    {
        private const string Database = "recruit";
        private const string Collection = "reviews";

        public MongoDbVacancyReviewRepository(
            ILogger<MongoDbVacancyReviewRepository> logger, IOptions<MongoDbConnectionDetails> details) 
            : base(logger, Database, Collection, details)
        {
        }

        public Task<List<VacancyReview>> SearchAsync(long vacancyReference)
        {
            var filterBuilder = Builders<VacancyReview>.Filter;

            var filter = (filterBuilder.Eq(r => r.Status, ReviewStatus.PendingReview)
                         | filterBuilder.Eq(r => r.Status, ReviewStatus.UnderReview)) 
                         & filterBuilder.Eq(r => r.VacancyReference, vacancyReference);
            return GetVacancyReviews(filter);
        }

        public Task<List<VacancyReview>> GetVacancyReviewsInProgressAsync(DateTime reviewExpiration)
        {
            var filterBuilder = Builders<VacancyReview>.Filter;
            var filter = filterBuilder.Eq(r => r.Status, ReviewStatus.UnderReview)
                & filterBuilder.Gt(r => r.ReviewedDate, reviewExpiration);
            return GetVacancyReviews(filter);
        }

        private async Task<List<VacancyReview>> GetVacancyReviews(FilterDefinition<VacancyReview> filter)
        {
            var collection = GetCollection<VacancyReview>();

            var result = await RetryPolicy
                .ExecuteAsync(
                    context => collection
                        .Find(filter)
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

        public async Task<List<VacancyReview>> GetByStatusAsync(ReviewStatus status)
        {
            var filter = Builders<VacancyReview>.Filter.Eq(r => r.Status, status);

            var collection = GetCollection<VacancyReview>();
            var result = await RetryPolicy.ExecuteAsync(context => collection
                .Find(filter)
                .ToListAsync(), new Context(nameof(GetByStatusAsync)));

            return result;
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

        public async Task<int> GetApprovedFirstTimeCountAsync(string submittedByUserId)
        {
            var filterBuilder = Builders<VacancyReview>.Filter;
            var filter = filterBuilder.Eq(r => r.SubmittedByUser.UserId, submittedByUserId) &
                         filterBuilder.Eq(r => r.Status, ReviewStatus.Closed) &
                         filterBuilder.Eq(r => r.ManualOutcome, ManualQaOutcome.Approved) &
                         filterBuilder.Eq(r => r.SubmissionCount, 1);

            var collection = GetCollection<VacancyReview>();
            var count = await RetryPolicy.ExecuteAsync(context => collection
                .CountDocumentsAsync(filter), 
                new Context(nameof(GetApprovedFirstTimeCountAsync)));

            return (int) count;
        }

        public async Task<int> GetApprovedCountAsync(string submittedByUserId)
        {
            var filterBuilder = Builders<VacancyReview>.Filter;
            var filter = filterBuilder.Eq(r => r.SubmittedByUser.UserId, submittedByUserId) &
                         filterBuilder.Eq(r => r.Status, ReviewStatus.Closed) &
                         filterBuilder.Eq(r => r.ManualOutcome, ManualQaOutcome.Approved);

            var collection = GetCollection<VacancyReview>();
            var count = await RetryPolicy.ExecuteAsync(context => collection
                    .CountDocumentsAsync(filter),
                new Context(nameof(GetApprovedCountAsync)));

            return (int)count;
        }

        public async Task<List<VacancyReview>> GetAssignedForUserAsync(string userId, DateTime assignationExpiryDateTime)
        {
            var filterBuilder = Builders<VacancyReview>.Filter;

            var filter = filterBuilder.Eq(r => r.Status, ReviewStatus.UnderReview)
                         & filterBuilder.Eq(r => r.ReviewedByUser.UserId, userId)
                         & filterBuilder.Gt(r => r.ReviewedDate, assignationExpiryDateTime);

            var collection = GetCollection<VacancyReview>();
            var result = await RetryPolicy.ExecuteAsync(context => collection
                .Find(filter)
                .ToListAsync(), new Context(nameof(GetAssignedForUserAsync)));

            return result.OrderByDescending(x => x.ReviewedDate).ToList();
        }

        public async Task<VacancyReview> GetCurrentReferredVacancyReviewAsync(long vacancyReference)
        {
            var vacancyReviews = await GetForVacancyAsync(vacancyReference);

            var vacancyReview = vacancyReviews.Where(r => r.Status == ReviewStatus.Closed &&
                                                          r.ManualOutcome == ManualQaOutcome.Referred)
                .OrderByDescending(r => r.ClosedDate)
                .FirstOrDefault();

            return vacancyReview;
        }
    }
}
