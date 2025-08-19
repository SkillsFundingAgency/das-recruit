using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;

namespace Esfa.Recruit.UnitTests.TestHelpers
{
    public class TestCache : ICache
    {
        public Task<T> CacheAsideAsync<T>(string key, DateTime absoluteExpiration, Func<Task<T>> objectToCache)
        {
            return objectToCache();
        }
    }
}
