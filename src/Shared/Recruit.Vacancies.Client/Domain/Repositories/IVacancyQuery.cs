using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Models;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories;

public interface IVacancyQuery
{
    [Obsolete]
    Task<IEnumerable<VacancyIdentifier>> GetVacanciesToCloseAsync(DateTime pointInTime);
    [Obsolete]
    Task<int> GetVacancyCountForUserAsync(string userId);
    [Obsolete]
    Task<Vacancy> GetVacancyAsync(Guid id);
    [Obsolete]
    Task<Vacancy> GetVacancyAsync(long vacancyReference);
    [Obsolete]
    Task<IList<Vacancy>> FindClosedVacancies(IList<long> vacancyReferences);
    [Obsolete]
    Task<IEnumerable<ProviderVacancySummary>> GetVacanciesAssociatedToProvider(long ukprn);
    [Obsolete]
    Task<IEnumerable<Vacancy>> GetProviderOwnedVacanciesForLegalEntityAsync(long ukprn, string accountLegalEntityPublicHashedId);
    [Obsolete]
    Task<IEnumerable<Vacancy>> GetProviderOwnedVacanciesInReviewAsync(long ukprn, string accountLegalEntityPublicHashedId);
    [Obsolete]
    Task<IEnumerable<Vacancy>> GetProviderOwnedVacanciesForEmployerWithoutAccountLegalEntityPublicHashedIdAsync(long ukprn, string employerAccountId);
    [Obsolete]
    Task<IEnumerable<T>> GetDraftVacanciesCreatedBeforeAsync<T>(DateTime staleDate);
    [Obsolete]
    Task<IEnumerable<T>> GetReferredVacanciesSubmittedBeforeAsync<T>(DateTime staleDate);
}