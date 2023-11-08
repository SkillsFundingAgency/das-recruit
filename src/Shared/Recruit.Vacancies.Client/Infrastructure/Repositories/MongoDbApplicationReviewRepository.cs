using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Polly;
using System.Linq;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    internal sealed class MongoDbApplicationReviewRepository : MongoDbCollectionBase, IApplicationReviewRepository, IApplicationReviewQuery
    {
        private class WrappedVacancyReference
        {
            public long VacancyReference { get; set; }
        }

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
            return RetryPolicy.Execute(_ =>
                collection.InsertOneAsync(review),
                new Context(nameof(CreateAsync)));
        }

        public async Task<ApplicationReview> GetAsync(Guid applicationReviewId)
        {
            var filter = Builders<ApplicationReview>.Filter.Eq(r => r.Id, applicationReviewId);
            var collection = GetCollection<ApplicationReview>();

            var result = await RetryPolicy.Execute(_ =>
                collection.Find(filter).SingleOrDefaultAsync(),
                new Context(nameof(GetAsync)));

            return result;
        }

        public async Task<List<T>> GetAllForSelectedIdsAsync<T>(List<Guid> applicationReviewIds)
        {
            var filter = Builders<T>.Filter.In(Id, applicationReviewIds);
            var collection = GetCollection<T>();

            var result = await RetryPolicy.Execute(_ =>
                collection.Find(filter)
                .Project<T>(GetProjection<T>())
                .ToListAsync(),
            new Context(nameof(GetAllForSelectedIdsAsync)));

            return result;
        }

        public async Task<ApplicationReview> GetAsync(long vacancyReference, Guid candidateId)
        {
            var builder = Builders<ApplicationReview>.Filter;
            var filter = builder.Eq(r => r.VacancyReference, vacancyReference) &
                         builder.Eq(r => r.CandidateId, candidateId);

            var collection = GetCollection<ApplicationReview>();

            var result = await RetryPolicy.Execute(_ =>
                collection.Find(filter).SingleOrDefaultAsync(),
                new Context(nameof(GetAsync)));

            return result;
        }

        public Task UpdateAsync(ApplicationReview applicationReview)
        {
            var filter = Builders<ApplicationReview>.Filter.Eq(r => r.Id, applicationReview.Id);
            var collection = GetCollection<ApplicationReview>();

            return RetryPolicy.Execute(_ =>
                collection.ReplaceOneAsync(filter, applicationReview),
                new Context(nameof(UpdateAsync)));
        }

        public async Task<UpdateResult> UpdateApplicationReviewsAsync(IEnumerable<Guid> applicationReviewIds, VacancyUser user, DateTime updatedDate, ApplicationReviewStatus status, string candidateFeedback)
        {
            var filter = Builders<ApplicationReview>.Filter.In(Id, applicationReviewIds);
            var collection = GetCollection<ApplicationReview>();

            var updateDef = new UpdateDefinitionBuilder<ApplicationReview>()
                .Set(appRev => appRev.Status, status)
                .Set(appRev => appRev.StatusUpdatedBy, user)
                .Set(appRev => appRev.StatusUpdatedDate, updatedDate);

            if (status == ApplicationReviewStatus.Unsuccessful && !string.IsNullOrEmpty(candidateFeedback))
            {
                updateDef = updateDef.Set(x => x.CandidateFeedback, candidateFeedback);
            }

            if (status.Equals(ApplicationReviewStatus.Shared))
            {
                updateDef = updateDef.Set(appRev => appRev.DateSharedWithEmployer, updatedDate);
            }

            return await RetryPolicy.Execute(_ =>
                collection.UpdateManyAsync(filter, updateDef),
            new Context(nameof(UpdateApplicationReviewsAsync)));
        }

        public async Task<List<T>> GetForVacancyAsync<T>(long vacancyReference)
        {
            var filter = Builders<T>.Filter.Eq(VacancyReference, vacancyReference);
            var collection = GetCollection<T>();

            var result = await RetryPolicy.Execute(_ =>
                collection.Find(filter)
                .Project<T>(GetProjection<T>())
                .ToListAsync(),
            new Context(nameof(GetForVacancyAsync)));

            return result;
        }

        public async Task<List<ApplicationReview>> GetForSharedVacancyAsync(long vacancyReference)
        {
            var filterBuilder = Builders<ApplicationReview>.Filter;
            var filter = filterBuilder.Eq(VacancyReference, vacancyReference) & filterBuilder.Ne(appRev => appRev.DateSharedWithEmployer, null);
            var collection = GetCollection<ApplicationReview>();

            var result = await RetryPolicy.Execute(_ =>
                collection.Find(filter)
                .Project<ApplicationReview>(GetProjection<ApplicationReview>())
                .ToListAsync(),
            new Context(nameof(GetForVacancyAsync)));

            return result;
        }

        public async Task<List<ApplicationReview>> GetForCandidateAsync(Guid candidateId)
        {
            var filter = Builders<ApplicationReview>.Filter.Eq(CandidateId, candidateId);
            var collection = GetCollection<ApplicationReview>();

            var result = await RetryPolicy.Execute(_ =>
                collection.Find(filter).ToListAsync(),
                new Context(nameof(GetForCandidateAsync)));

            return result;
        }

        public Task HardDelete(Guid applicationReviewId)
        {
            var filter = Builders<ApplicationReview>.Filter.Eq(Id, applicationReviewId);
            var collection = GetCollection<ApplicationReview>();

            return RetryPolicy.Execute(_ =>
                collection.DeleteOneAsync(filter),
                new Context(nameof(HardDelete)));
        }

        public async Task<IEnumerable<long>> GetAllVacancyReferencesAsync()
        {
            var filter = Builders<ApplicationReview>.Filter.Empty;

            var collection = GetCollection<ApplicationReview>();

            var result = await RetryPolicy.Execute(async _ =>
                                                        {
                                                            var pipeline = GetDistinctVacancyReferencesPipeline();
                                                            var aggResults = await collection.AggregateAsync<WrappedVacancyReference>(pipeline);
                                                            return (await aggResults.ToListAsync()).Select(x => x.VacancyReference).ToList();
                                                        },
                                                        new Context(nameof(GetForVacancyAsync)));

            return result;
        }

        private BsonDocument[] GetDistinctVacancyReferencesPipeline()
        {
            const string FieldName = "vacancyReference";

            return new[]
            {
                new BsonDocument().Add("$group", new BsonDocument
                {
                    {
                        "_id", new BsonDocument
                        {
                            { FieldName, $"${FieldName}" }
                        }
                    },
                    { FieldName, new BsonDocument().Add("$first", $"${FieldName}") }
                }),
                new BsonDocument().Add("$project", new BsonDocument
                {
                    { "_id", 0 },
                    { FieldName, 1 }
                })
            };
        }
    }
}