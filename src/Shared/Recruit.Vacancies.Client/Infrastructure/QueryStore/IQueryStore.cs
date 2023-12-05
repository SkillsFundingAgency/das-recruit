using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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

        Task<long> DeleteManyLessThanAsync<T, T1>(string typeName, Expression<Func<T, T1>> property, T1 value) where T : QueryProjectionBase;

        Task<long> DeleteAllAsync<T>(string typeName) where T : QueryProjectionBase;
        Task<IEnumerable<LiveVacancy>> GetAllLiveExpired(DateTime? closingDate);
        Task<IEnumerable<LiveVacancy>> GetAllLiveVacancies(int vacanciesToSkip, int vacanciesToGet);
        Task<long> GetAllLiveVacanciesCount();
        Task<LiveVacancy> GetLiveVacancy(long vacancyReference);
    }
}