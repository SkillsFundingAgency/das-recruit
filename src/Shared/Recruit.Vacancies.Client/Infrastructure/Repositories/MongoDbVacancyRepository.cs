using System;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using Esfa.Recruit.Vacancies.Client.Domain.Models;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    internal class MongoDbVacancyRepository : MongoDbCollectionBase, IVacancyRepository, IVacancyQuery
    {
        private Vacancy _singleOrDefaultAsync;
        private const string EmployerAccountIdFieldName = "employerAccountId";
        private const string ProviderUkprnFieldName = "trainingProvider.ukprn";
        private const string OwnerTypeFieldName = "ownerType";
        private const string IsDeletedFieldName = "isDeleted";
        private const string VacancyStatusFieldName = "status";
        private const string VacancyReferenceFieldName = "vacancyReference";
        private const string CreatedByUserId = "createdByUser.userId";
        private const string SubmittedByUserId = "submittedByUser.userId";

        public MongoDbVacancyRepository(ILoggerFactory loggerFactory, IOptions<MongoDbConnectionDetails> details)
            : base(loggerFactory, MongoDbNames.RecruitDb, MongoDbCollectionNames.Vacancies, details)
        {
        }

        public Task CreateAsync(Vacancy vacancy)
        {
            var collection = GetCollection<Vacancy>();
            return RetryPolicy.ExecuteAsync(_ =>
                collection.InsertOneAsync(vacancy),
                new Context(nameof(CreateAsync)));
        }

        public async Task<Vacancy> GetVacancyAsync(long vacancyReference)
        {
            var vacancy = await FindVacancy(v => v.VacancyReference, vacancyReference);

            if (vacancy == null)
                throw new VacancyNotFoundException(string.Format(ExceptionMessages.VacancyWithReferenceNotFound, vacancyReference));

            return vacancy;
        }

        public async Task<Vacancy> GetVacancyAsync(Guid id)
        {
            var vacancy = await FindVacancy(v => v.Id, id);

            if (vacancy == null)
                throw new VacancyNotFoundException(string.Format(ExceptionMessages.VacancyWithIdNotFound, id));

            return vacancy;
        }

        private async Task<Vacancy> FindVacancy<TField>(Expression<Func<Vacancy, TField>> expression, TField value)
        {
            var filter = Builders<Vacancy>.Filter.Eq(expression, value);
            var collection = GetCollection<Vacancy>();

            var result = await RetryPolicy.ExecuteAsync(async _ => await collection.Find(filter).SingleOrDefaultAsync(),
                new Context(nameof(FindVacancy)));

            return result;
        }

        public async Task<IList<Vacancy>> FindClosedVacancies(IList<long> vacancyReferences)
        {
            var collection = GetCollection<Vacancy>();
            var builderFilter = Builders<Vacancy>.Filter;
            var filter =  builderFilter.In<string>("status", new List<string>{VacancyStatus.Closed.ToString(),VacancyStatus.Live.ToString()}) 
                          & builderFilter.In<long>(identifier => identifier.VacancyReference.Value, vacancyReferences);
            
            var result = await RetryPolicy.ExecuteAsync(async _ =>
                    await collection.Find(filter).ToListAsync(),
                new Context(nameof(FindVacancy)));

            return result;
        }

        public async Task<int> GetVacancyCountForUserAsync(string userId)
        {
            var builder = Builders<Vacancy>.Filter;
            var filter = builder.Eq(CreatedByUserId, userId) |
                        builder.Eq(SubmittedByUserId, userId);

            var collection = GetCollection<Vacancy>();

            var result = await RetryPolicy.ExecuteAsync(_ =>
                    collection.CountDocumentsAsync(filter),
                new Context(nameof(GetVacancyCountForUserAsync)));

            return (int)result;
        }

        public async Task<IEnumerable<T>> GetDraftVacanciesCreatedBeforeAsync<T>(DateTime staleByDate)
        {
            var builder = Builders<T>.Filter;
            var filter = builder.Eq("status", VacancyStatus.Draft.ToString()) &
                        builder.Lt("createdDate", staleByDate) &
                        builder.Eq(IsDeletedFieldName, false);
                        var collection = GetCollection<T>();

            var result = await RetryPolicy.ExecuteAsync(_ =>
                    collection.Find(filter)
                    .Project<T>(GetProjection<T>())
                    .ToListAsync(),
            new Context(nameof(GetDraftVacanciesCreatedBeforeAsync)));

            return result;
        }

        public async Task<IEnumerable<T>> GetReferredVacanciesSubmittedBeforeAsync<T>(DateTime staleByDate)
        {
            var builder = Builders<T>.Filter;
            var filter = builder.Eq("status", VacancyStatus.Referred.ToString()) &
                        builder.Lt("submittedDate", staleByDate) &
                        builder.Eq(IsDeletedFieldName, false);
                        var collection = GetCollection<T>();

            var result = await RetryPolicy.ExecuteAsync(_ =>
                    collection.Find(filter)
                    .Project<T>(GetProjection<T>())
                    .ToListAsync(),
            new Context(nameof(GetReferredVacanciesSubmittedBeforeAsync)));

            return result;
        }

        public Task<IEnumerable<VacancyIdentifier>> GetVacanciesToCloseAsync(DateTime pointInTime)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(Vacancy vacancy)
        {
            var filter = Builders<Vacancy>.Filter.Eq(v => v.Id, vacancy.Id);
            var collection = GetCollection<Vacancy>();
            await RetryPolicy.ExecuteAsync(async _ =>
                await collection.ReplaceOneAsync(filter, vacancy),
                new Context(nameof(UpdateAsync)));
        }

        public async Task<IEnumerable<ProviderVacancySummary>> GetVacanciesAssociatedToProvider(long ukprn)
        {
            var builder = Builders<Vacancy>.Filter;
            var filter = 
                builder.Eq(ProviderUkprnFieldName, ukprn) &
                builder.Ne(IsDeletedFieldName, true);

            var collection = GetCollection<Vacancy>();

            var result = await RetryPolicy.ExecuteAsync(_ =>
                collection
                    .Find(filter)
                    .Project(x => new ProviderVacancySummary
                    {
                        Id = x.Id,
                        VacancyOwner = x.OwnerType,
                        VacancyReference = x.VacancyReference.GetValueOrDefault(),
                        Status = x.Status,
                        EmployerAccountId = x.EmployerAccountId
                    })
                    .ToListAsync(),
                new Context(nameof(GetVacanciesAssociatedToProvider)));

            return result;
        }

        public async Task<IEnumerable<Vacancy>> GetProviderOwnedVacanciesForLegalEntityAsync(long ukprn, string accountLegalEntityPublicHashedId)
        {
            var builder = Builders<Vacancy>.Filter;
            var filter = builder.Eq(v => v.IsDeleted, false) &
                         builder.Eq(v => v.OwnerType, OwnerType.Provider) &
                         builder.Eq(v => v.TrainingProvider.Ukprn, ukprn) &
                         builder.Eq(v => v.AccountLegalEntityPublicHashedId, accountLegalEntityPublicHashedId);

            var collection = GetCollection<Vacancy>();

            var result = await RetryPolicy.ExecuteAsync(_ =>
                collection.Aggregate()
                            .Match(filter)
                            .ToListAsync(),
                new Context(nameof(GetProviderOwnedVacanciesForLegalEntityAsync)));

            return result;
        }

        public async Task<IEnumerable<Vacancy>> GetProviderOwnedVacanciesInReviewAsync(long ukprn, string accountLegalEntityPublicHashedId)
        {
            var builder = Builders<Vacancy>.Filter;
            var filter = builder.Eq(v => v.IsDeleted, false) &
                         builder.Eq(v => v.OwnerType, OwnerType.Provider) &
                         builder.Eq(v => v.TrainingProvider.Ukprn, ukprn) &
                         builder.Eq(v => v.AccountLegalEntityPublicHashedId, accountLegalEntityPublicHashedId) &
                         builder.Eq(v => v.Status, VacancyStatus.Review);

            var collection = GetCollection<Vacancy>();

            var result = await RetryPolicy.ExecuteAsync(_ =>
                    collection.Aggregate()
                        .Match(filter)
                        .ToListAsync(),
                new Context(nameof(GetProviderOwnedVacanciesInReviewAsync)));

            return result;
        }

        public async Task<IEnumerable<Vacancy>> GetProviderOwnedVacanciesForEmployerWithoutAccountLegalEntityPublicHashedIdAsync(long ukprn, string employerAccountId)
        {
            var builder = Builders<Vacancy>.Filter;
            var filter = builder.Eq(v => v.IsDeleted, false) &
                         builder.Eq(v => v.OwnerType, OwnerType.Provider) &
                         builder.Eq(v => v.TrainingProvider.Ukprn, ukprn) &
                         builder.Eq(v => v.EmployerAccountId, employerAccountId) &
                         builder.Eq(v => v.AccountLegalEntityPublicHashedId, "0");

            var collection = GetCollection<Vacancy>();

            var result = await RetryPolicy.ExecuteAsync(_ =>
                    collection.Aggregate()
                        .Match(filter)
                        .ToListAsync(),
                new Context(nameof(GetProviderOwnedVacanciesForEmployerWithoutAccountLegalEntityPublicHashedIdAsync)));

            return result;
        }
    }
}