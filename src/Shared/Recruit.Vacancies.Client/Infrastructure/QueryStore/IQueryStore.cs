using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    public interface IQueryStore
    {
        Task<T> GetAsync<T>(string key) where T : QueryProjectionBase;

        Task UpsertAsync<T>(T item) where T : QueryProjectionBase;
    }
}