using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    public interface IQueryStore
    {
        Task<T> GetAsync<T>(string key) where T : class, IQueryProjection;

        Task UpsertAsync<T>(T item) where T : class, IQueryProjection;
    }
}