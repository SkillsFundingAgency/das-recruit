using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    }
}