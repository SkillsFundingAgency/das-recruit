using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    public interface IQueryStore
    {
        Task<T> GetAsync<T>(string key) where T : QueryProjectionBase;
        Task<T> GetAsync<T>(string typeName, string key) where T : QueryProjectionBase;
        Task UpsertAsync<T>(T item) where T : QueryProjectionBase;
        Task DeleteAsync<T>(string typeName, string key) where T : QueryProjectionBase;
        Task<IEnumerable<LiveVacancy>> GetAllLiveExpired(DateTime? closingDate);
    }
}