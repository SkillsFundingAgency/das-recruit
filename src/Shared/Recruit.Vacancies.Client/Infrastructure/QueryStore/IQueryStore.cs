using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    public interface IQueryStore
    {
        Task<IEnumerable<T>> GetAllByTypeAsync<T>(string typeName) where T : QueryProjectionBase;

        Task<T> GetAsync<T>(string typeName, string key) where T : QueryProjectionBase;

        Task UpsertAsync<T>(T item) where T : QueryProjectionBase;

        Task RecreateAsync<T>(string typeName, IList<T> items) where T : QueryProjectionBase;

        Task DeleteAsync<T>(string typeName, string key) where T : QueryProjectionBase;

        Task<long> DeleteManyAsync<T, T1>(string typeName, Expression<Func<T, T1>> property, T1 value) where T : QueryProjectionBase;
    }
}