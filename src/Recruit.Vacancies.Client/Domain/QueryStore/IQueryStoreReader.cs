using Esfa.Recruit.Storage.Client.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Storage.Client.Domain.QueryStore
{
    public interface IQueryStoreReader
    {
        // E.g. Task<object> GetDashboardAsync(string key);
        Task<IEnumerable<Vacancy>> GetVacanciesAsync(string employerAccountId);
    }
}