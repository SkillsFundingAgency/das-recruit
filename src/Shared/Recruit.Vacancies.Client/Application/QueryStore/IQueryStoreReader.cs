using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.QueryStore
{
    public interface IQueryStoreReader
    {
        Task<Dashboard> GetDashboardAsync(string employerAccountId);
    }
}