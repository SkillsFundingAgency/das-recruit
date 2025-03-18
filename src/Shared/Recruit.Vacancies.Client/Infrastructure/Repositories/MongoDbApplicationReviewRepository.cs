﻿using System;
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
using Esfa.Recruit.Vacancies.Client.Infrastructure.Extensions;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories;

internal sealed class MongoDbApplicationReviewRepository(
    ILoggerFactory loggerFactory,
    IOptions<MongoDbConnectionDetails> details)
    : MongoDbCollectionBase(loggerFactory, MongoDbNames.RecruitDb, MongoDbCollectionNames.ApplicationReviews, details),
        IApplicationReviewRepository, IApplicationReviewQuery
{
    private class WrappedVacancyReference
    {
        public long VacancyReference { get; set; }
    }

    private const string VacancyReference = "vacancyReference";
    private const string CandidateId = "candidateId";
    private const string Id = "_id";

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

    public async Task<List<ApplicationReview>> GetByStatusAsync(long vacancyReference, ApplicationReviewStatus status)
    {
        var builder = Builders<ApplicationReview>.Filter;
        var filter = builder.Eq(r => r.VacancyReference, vacancyReference) &
                     builder.Eq(r => r.Status, status);

        var collection = GetCollection<ApplicationReview>();

        var result = await RetryPolicy.ExecuteAsync(_ =>
                collection.Find(filter)
                    .Project<ApplicationReview>(GetProjection<ApplicationReview>())
                    .ToListAsync(),
            new Context(nameof(GetByStatusAsync)));

        return result;
    }

    public async Task<List<T>> GetAllForSelectedIdsAsync<T>(List<Guid> applicationReviewIds)
    {
        var filter = Builders<T>.Filter.In(Id, applicationReviewIds);
        var collection = GetCollection<T>();

        var result = await RetryPolicy.ExecuteAsync(_ =>
                collection.Find(filter)
                    .Project<T>(GetProjection<T>())
                    .ToListAsync(),
            new Context(nameof(GetAllForSelectedIdsAsync)));

        return result;
    }
        
    public async Task<List<ApplicationReview>> GetAllForVacancyWithTemporaryStatus(long vacancyReference, ApplicationReviewStatus status)
    {
        var builder = Builders<ApplicationReview>.Filter;
        var filter = builder.Eq(r => r.TemporaryReviewStatus, status) &
                     builder.Eq(r => r.VacancyReference, vacancyReference);
            
        var collection = GetCollection<ApplicationReview>();

        var result = await RetryPolicy.ExecuteAsync(_ =>
                collection.Find(filter)
                    .Project<ApplicationReview>(GetProjection<ApplicationReview>())
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

    public async Task<UpdateResult> UpdateApplicationReviewsAsync(IEnumerable<Guid> applicationReviewIds, VacancyUser user, DateTime updatedDate, ApplicationReviewStatus? status, ApplicationReviewStatus? temporaryReviewStatus, string candidateFeedback = null, long? vacancyReference = null)
    {
        if (vacancyReference.HasValue)
        {
            await ClearApplicationReviewsTemporaryStatus(vacancyReference.Value, user, updatedDate);
        }
            
        var builder = Builders<ApplicationReview>.Filter;
        var filter = builder.In(Id, applicationReviewIds) 
                     & builder.Eq(r => r.IsWithdrawn, false);
        var collection = GetCollection<ApplicationReview>();

        var updateDef = new UpdateDefinitionBuilder<ApplicationReview>()
            .Set(appRev => appRev.StatusUpdatedBy, user)
            .Set(appRev => appRev.StatusUpdatedDate, updatedDate);

        if (status != null)
        {
            updateDef = updateDef.Set(appRev => appRev.Status, status);
            updateDef = updateDef.Set(appRev => appRev.TemporaryReviewStatus, null);
                
            updateDef = status switch
            {
                ApplicationReviewStatus.Unsuccessful when !string.IsNullOrEmpty(candidateFeedback) => updateDef.Set(x => x.CandidateFeedback, candidateFeedback),
                ApplicationReviewStatus.Shared => updateDef.Set(appRev => appRev.DateSharedWithEmployer, updatedDate),
                _ => updateDef
            };
        }

        if (temporaryReviewStatus != null)
        {
            updateDef = updateDef.Set(appRev => appRev.TemporaryReviewStatus, temporaryReviewStatus);
        }

        return await RetryPolicy.ExecuteAsync(_ =>
                collection.UpdateManyAsync(filter, updateDef),
            new Context(nameof(UpdateApplicationReviewsAsync)));
    }

    private async Task<UpdateResult> ClearApplicationReviewsTemporaryStatus(long vacancyReference, VacancyUser user, DateTime updatedDate)
    {
        var builder = Builders<ApplicationReview>.Filter;
        var filter = builder.Eq(r => r.VacancyReference, vacancyReference) 
                     & builder.Ne(r => r.TemporaryReviewStatus, null);
        var collection = GetCollection<ApplicationReview>();
        var updateDef = new UpdateDefinitionBuilder<ApplicationReview>()    
            .Set(appRev => appRev.TemporaryReviewStatus, null)
            .Set(appRev => appRev.CandidateFeedback, null)
            .Set(appRev => appRev.StatusUpdatedBy, user)
            .Set(appRev => appRev.StatusUpdatedDate, updatedDate);
            
        return await RetryPolicy.ExecuteAsync(_ =>
                collection.UpdateManyAsync(filter, updateDef),
            new Context(nameof(UpdateApplicationReviewsPendingUnsuccessfulFeedback)));
    }

    public async Task<UpdateResult> UpdateApplicationReviewsPendingUnsuccessfulFeedback(long vacancyReference, VacancyUser user, DateTime updatedDate, string candidateFeedback)
    {
        var builder = Builders<ApplicationReview>.Filter;
        var filter = builder.Eq(r => r.VacancyReference, vacancyReference) 
                     & builder.Eq(r => r.IsWithdrawn, false)
                     & builder.Eq(r => r.TemporaryReviewStatus, ApplicationReviewStatus.PendingToMakeUnsuccessful);
        var collection = GetCollection<ApplicationReview>();
        var updateDef = new UpdateDefinitionBuilder<ApplicationReview>()
            .Set(appRev => appRev.CandidateFeedback, candidateFeedback)
            .Set(appRev => appRev.StatusUpdatedBy, user)
            .Set(appRev => appRev.StatusUpdatedDate, updatedDate);
            
        return await RetryPolicy.ExecuteAsync(_ =>
                collection.UpdateManyAsync(filter, updateDef),
            new Context(nameof(UpdateApplicationReviewsPendingUnsuccessfulFeedback)));
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

    public async Task<List<ApplicationReview>> GetForVacancySortedAsync(long vacancyReference, SortColumn sortColumn, SortOrder sortOrder)
    {
        var filter = Builders<ApplicationReview>.Filter.Eq(VacancyReference, vacancyReference);
        var collection = GetCollection<ApplicationReview>();

        var result = await RetryPolicy.ExecuteAsync(_ =>
                collection.Find(filter)
                    .Project<ApplicationReview>(GetProjection<ApplicationReview>())
                    .ToListAsync(),
            new Context(nameof(GetForVacancyAsync)));

        var sortedResult = result.AsQueryable()
            .Sort(sortColumn, sortOrder);

        return sortedResult.ToList();
    }
    
    public async Task<long> GetCountForVacancyAsync(long vacancyReference)
    {
        var filter = Builders<ApplicationReview>.Filter.Eq(VacancyReference, vacancyReference);
        var collection = GetCollection<ApplicationReview>();
        long result = await RetryPolicy.ExecuteAsync(_ => collection.Find(filter).CountDocumentsAsync(), new Context(nameof(GetForVacancyAsync)));
        return result;
    }

    public async Task<List<ApplicationReview>> GetForVacancyAsync(long vacancyReference, SortColumn sortColumn, SortOrder sortOrder, int skip, int take)
    {
        var filter = Builders<ApplicationReview>.Filter.Eq(VacancyReference, vacancyReference);
        var collection = GetCollection<ApplicationReview>();
        
        var result = await RetryPolicy
            .ExecuteAsync(
                _ => collection
                    .Find(filter)
                    .Project<ApplicationReview>(GetProjection<ApplicationReview>())
                    .ToListAsync(),
                new Context(nameof(GetForVacancyAsync))
            );
        
        return result
            .AsQueryable()
            .Sort(sortColumn, sortOrder)
            .Skip(skip)
            .Take(take)
            .ToList();
    }
    
    public async Task<long> GetCountForSharedVacancyAsync(long vacancyReference)
    {
        var filterBuilder = Builders<ApplicationReview>.Filter;
        var filter = filterBuilder.Eq(VacancyReference, vacancyReference) & filterBuilder.Ne(appRev => appRev.DateSharedWithEmployer, null);
        var collection = GetCollection<ApplicationReview>();
        long result = await RetryPolicy.ExecuteAsync(_ => collection.Find(filter).CountDocumentsAsync(), new Context(nameof(GetForVacancyAsync)));
        return result;
    }
        
    public async Task<List<ApplicationReview>> GetForSharedVacancyAsync(long vacancyReference, SortColumn sortColumn, SortOrder sortOrder, int skip, int take)
    {
        var filterBuilder = Builders<ApplicationReview>.Filter;
        var filter = filterBuilder.Eq(VacancyReference, vacancyReference) & filterBuilder.Ne(appRev => appRev.DateSharedWithEmployer, null);
        var collection = GetCollection<ApplicationReview>();

        var result = await RetryPolicy
            .ExecuteAsync(
                _ => collection
                    .Find(filter)
                    .Project<ApplicationReview>(GetProjection<ApplicationReview>())
                    .ToListAsync(),
                new Context(nameof(GetForVacancyAsync))
            );

        return result
            .AsQueryable()
            .Sort(sortColumn, sortOrder, true)
            .Skip(skip)
            .Take(take)
            .ToList();
    }

    public async Task<List<ApplicationReview>> GetForSharedVacancySortedAsync(long vacancyReference, SortColumn sortColumn, SortOrder sortOrder)
    {
        var filterBuilder = Builders<ApplicationReview>.Filter;
        var filter = filterBuilder.Eq(VacancyReference, vacancyReference) & filterBuilder.Ne(appRev => appRev.DateSharedWithEmployer, null);
        var collection = GetCollection<ApplicationReview>();

        var result = await RetryPolicy.ExecuteAsync(_ =>
                collection.Find(filter)
                    .Project<ApplicationReview>(GetProjection<ApplicationReview>())
                    .ToListAsync(),
            new Context(nameof(GetForVacancyAsync)));

        var sortedResult = result.AsQueryable()
            .Sort(sortColumn, sortOrder, true);

        return sortedResult.ToList();
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

    public async Task<IEnumerable<long>> GetAllVacancyReferencesAsync()
    {
        var filter = Builders<ApplicationReview>.Filter.Empty;

        var collection = GetCollection<ApplicationReview>();

        var result = await RetryPolicy.ExecuteAsync(async _ =>
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