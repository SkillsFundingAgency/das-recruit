using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
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
        private readonly ITimeProvider _timeProvider;
        private const string Database = "recruit";
        private const string Collection = "vacancyReviews";

        public MongoDbVacancyReviewRepository(
            ILogger<MongoDbVacancyReviewRepository> logger, IOptions<MongoDbConnectionDetails> details, 
            ITimeProvider timeProvider) 
            : base(logger, Database, Collection, details)
        {
            _timeProvider = timeProvider;
        }

        public Task<List<QaVacancySummary>> SearchAsync(long vacancyReference)
        {
            var filterBuilder = Builders<VacancyReview>.Filter;

            var filter = (filterBuilder.Eq(r => r.Status, ReviewStatus.PendingReview)
                         | filterBuilder.Eq(r => r.Status, ReviewStatus.UnderReview)) 
                         & filterBuilder.Eq(r => r.VacancyReference, vacancyReference);
            return GetQaVacancySummaries(filter);
        }

        public Task<List<QaVacancySummary>> GetVacancyReviewsInProgressAsync()
        {
            var reviewExpiration = _timeProvider.Now.AddHours(-3);
            var filterBuilder = Builders<VacancyReview>.Filter;
            var filter = filterBuilder.Eq(r => r.Status, ReviewStatus.UnderReview)
                & filterBuilder.Gt(r => r.ReviewedDate, reviewExpiration);
            return GetQaVacancySummaries(filter);
        }

        private async Task<List<QaVacancySummary>> GetQaVacancySummaries(FilterDefinition<VacancyReview> filter)
        {
            var collection = GetCollection<VacancyReview>();

            var result = await RetryPolicy
                .ExecuteAsync(
                    context => collection
                        .Find(filter)
                        .Project(GetQaVacancySummaryProjection())
                        .ToListAsync(),
                    new Context(nameof(SearchAsync)))
                .ConfigureAwait(false);

            return result;
        }

        private ProjectionDefinition<VacancyReview, QaVacancySummary> GetQaVacancySummaryProjection()
        {
            return Builders<VacancyReview>.Projection.Expression(r =>
                new QaVacancySummary()
                {
                    Id = r.Id,
                    Title = r.Title,
                    VacancyReference = r.VacancyReference,
                    ReviewAssignedToUserName = r.ReviewedByUser.Name,
                    ReviewAssignedToUserId = r.ReviewedByUser.UserId,
                    ReviewStartedOn = r.ReviewedDate,
                    EmployerName = r.VacancySnapshot != null ? r.VacancySnapshot.EmployerName : null,
                    ClosingDate = r.VacancySnapshot != null ? r.VacancySnapshot.ClosingDate.GetValueOrDefault() : DateTime.MinValue,
                    Status = r.Status,
                    SubmittedDate = r.VacancySnapshot != null ? r.VacancySnapshot.SubmittedDate.GetValueOrDefault() : DateTime.MinValue
                });

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
                .CountAsync(filter), 
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
                    .CountAsync(filter),
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
    }
}
