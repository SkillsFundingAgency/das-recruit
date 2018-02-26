using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Domain.QueryStore
{
    public interface IQueryStoreReader
    {
        Task<Dashboard> GetDashboardAsync(string employerAccountId);
    }
}