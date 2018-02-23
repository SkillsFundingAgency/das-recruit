using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.QueryStore
{
    public interface IQueryStoreReader
    {
        // E.g. Task<object> GetDashboardAsync(string key);
        Task<IEnumerable<Vacancy>> GetVacanciesAsync(string employerAccountId);
    }
}