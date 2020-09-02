using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Models;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories
{
    public interface IVacancyQuery
    {
        Task<IEnumerable<T>> GetVacanciesByEmployerAccountAsync<T>(string employerAccountId);
        Task<IEnumerable<T>> GetVacanciesByProviderAccountAsync<T>(long ukprn);
        Task<IEnumerable<T>> GetVacanciesByStatusAsync<T>(VacancyStatus status);
        Task<int> GetVacancyCountForUserAsync(string userId);
        Task<Vacancy> GetSingleVacancyForPostcodeAsync(string postcode);
        Task<Vacancy> GetVacancyAsync(Guid id);
        Task<IEnumerable<string>> GetDistinctVacancyOwningEmployerAccountsAsync();
        Task<IEnumerable<long>> GetDistinctVacancyOwningProviderAccountsAsync();
        Task<IEnumerable<long>> GetAllVacancyReferencesAsync();
        Task<IEnumerable<Guid>> GetAllVacancyIdsAsync();
        Task<IEnumerable<ProviderVacancySummary>> GetVacanciesAssociatedToProvider(long ukprn);
        Task<IEnumerable<Vacancy>> GetProviderOwnedVacanciesForLegalEntityAsync(long ukprn, string accountLegalEntityPublicHashedId);
        Task<IEnumerable<Vacancy>> GetProviderOwnedVacanciesForEmployerWithoutAccountLegalEntityPublicHashedIdAsync(long ukprn, string employerAccountId);
        Task<IEnumerable<T>> GetDraftVacanciesCreatedBeforeAsync<T>(DateTime staleDate);
        Task<IEnumerable<T>> GetReferredVacanciesSubmittedBeforeAsync<T>(DateTime staleDate);
    }
}