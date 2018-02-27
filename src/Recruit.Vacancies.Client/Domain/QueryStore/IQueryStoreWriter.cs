using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Domain.QueryStore
{
    public interface IQueryStoreWriter
    {
         Task UpdateDashboardAsync(Dashboard dashboard); 
    }
}