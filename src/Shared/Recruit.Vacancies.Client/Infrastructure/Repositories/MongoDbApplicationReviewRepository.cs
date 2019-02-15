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
    internal sealed class MongoDbApplicationReviewRepository : MongoDbCollectionBase, IApplicationReviewRepository, IApplicationReviewQuery
    {
        private const string VacancyReference = "vacancyReference";
        private const string CandidateId = "candidateId";
        private const string Id = "_id";

        public MongoDbApplicationReviewRepository(ILoggerFactory loggerFactory, IOptions<MongoDbConnectionDetails> details)
            : base(loggerFactory, MongoDbNames.RecruitDb, MongoDbCollectionNames.ApplicationReviews, details)
        {
        }

        public Task CreateAsync(ApplicationReview review)
        {
            var collection = GetCollection<ApplicationReview>();
            return RetryPolicy.ExecuteAsync(_ => 
                collection.InsertOneAsync(review),
                new Context(nameof(CreateAsync)));
        }

        public async Task<ApplicationReview> GetAsync(Guid applicationReviewId)
        {
            var filter = Builders<ApplicationReview>.Filter.Eq(r => r.Id, applicationReviewId);
            var collection = GetCollection<ApplicationReview>();

            var result = await RetryPolicy.ExecuteAsync(_ => 
                collection.Find(filter).SingleOrDefaultAsync(),
                new Context(nameof(GetAsync)));

            return result;
        }

        public async Task<ApplicationReview> GetAsync(long vacancyReference, Guid candidateId)
        {
            var builder = Builders<ApplicationReview>.Filter;
            var filter = builder.Eq(r => r.VacancyReference, vacancyReference) &
                         builder.Eq(r => r.CandidateId, candidateId);
            
            var collection = GetCollection<ApplicationReview>();

            var result = await RetryPolicy.ExecuteAsync(_ => 
                collection.Find(filter).SingleOrDefaultAsync(),
                new Context(nameof(GetAsync)));

            return result;
        }

        public Task UpdateAsync(ApplicationReview applicationReview)
        {
            var filter = Builders<ApplicationReview>.Filter.Eq(r => r.Id, applicationReview.Id);
            var collection = GetCollection<ApplicationReview>();

            return RetryPolicy.ExecuteAsync(_ => 
                collection.ReplaceOneAsync(filter, applicationReview),
                new Context(nameof(UpdateAsync)));
        }

        public async Task<List<ApplicationReviewCount>> GetStatusCountsForEmployerAsync(string employerAccountId)
        {
            var collection = GetCollection<ApplicationReview>();

            var builder = Builders<ApplicationReview>.Filter;
            var filter = builder.Eq(r => r.EmployerAccountId, employerAccountId) &
                         builder.Ne(r => r.IsWithdrawn, true);

            var result = await RetryPolicy.ExecuteAsync(_ =>
            {
                var aggregate = collection.Aggregate()

                    .Match(filter)

                    .Group(groupBy => new ApplicationReviewCount.ApplicationReviewsCountGroupKey {VacancyReference = groupBy.VacancyReference, Status = groupBy.Status},
                        g =>
                            new ApplicationReviewCount {Id = g.Key, Count = g.Count()})
                    .ToListAsync();

                return aggregate;
            },
            new Context(nameof(GetStatusCountsForEmployerAsync)));

            return result;
        }

        public async Task<List<ApplicationReviewCount>> GetStatusCountsForProviderAsync(long ukprn)
        {
            //var collection = GetCollection<ApplicationReview>();

            //var builder = Builders<ApplicationReview>.Filter;
            //var filter = builder.Eq(r => r.Ukprn, ukprn) &
            //             builder.Ne(r => r.IsWithdrawn, true);

            //var result = await RetryPolicy.ExecuteAsync(_ =>
            //{
            //    var aggregate = collection.Aggregate()

            //        .Match(filter)

            //        .Group(groupBy => new ApplicationReviewCount.ApplicationReviewsCountGroupKey {VacancyReference = groupBy.VacancyReference, Status = groupBy.Status},
            //            g =>
            //                new ApplicationReviewCount {Id = g.Key, Count = g.Count()})
            //        .ToListAsync();

            //    return aggregate;
            //},
            //new Context(nameof(GetStatusCountsForProviderAsync)));

            //return result;

            return await Task.FromResult(new List<ApplicationReviewCount>());
        }

        public async Task<List<T>> GetForVacancyAsync<T>(long vacancyReference)
        {
            var filter = Builders<T>.Filter.Eq(VacancyReference, vacancyReference);
            var collection = GetCollection<T>();
            
            var result = await RetryPolicy.ExecuteAsync(_ => 
                collection.Find(filter)
                .Project<T>(GetProjection<T>())
                .ToListAsync(),
            new Context(nameof(GetForVacancyAsync)));

            return result;
        }

        public async Task<List<ApplicationReview>> GetForCandidateAsync(Guid candidateId)
        {
            var filter = Builders<ApplicationReview>.Filter.Eq(CandidateId, candidateId);
            var collection = GetCollection<ApplicationReview>();

            var result = await RetryPolicy.ExecuteAsync(_ => 
                collection.Find(filter).ToListAsync(),
                new Context(nameof(GetForCandidateAsync)));

            return result;
        }

        public Task HardDelete(Guid applicationReviewId)
        {
            var filter = Builders<ApplicationReview>.Filter.Eq(Id, applicationReviewId);
            var collection = GetCollection<ApplicationReview>();

            return RetryPolicy.ExecuteAsync(_ => 
                collection.DeleteOneAsync(filter),
                new Context(nameof(HardDelete)));
        }
    }
}