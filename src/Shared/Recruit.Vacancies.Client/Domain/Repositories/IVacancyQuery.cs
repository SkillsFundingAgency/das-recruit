using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories
{
    public interface IVacancyQuery
    {
        Task<IEnumerable<T>> GetVacanciesByEmployerAccountAsync<T>(string employerAccountId);
        Task<Vacancy> GetSingleVacancyForPostcodeAsync(string postcode);
        Task<IEnumerable<Vacancy>> GetVacanciesByStatusAsync(VacancyStatus status);
        Task<IEnumerable<string>> GetDistinctEmployerAccountsAsync();
    }
}