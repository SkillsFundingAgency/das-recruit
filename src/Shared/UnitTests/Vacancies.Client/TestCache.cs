using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client
{
    public class TestCache : ICache
    {
        public Task<T> CacheAsideAsync<T>(string key, DateTime absoluteExpiration, Func<Task<T>> objectToCache)
        {
            return objectToCache();
        }
    }
}
