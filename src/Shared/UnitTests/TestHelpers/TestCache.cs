using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.TestHelpers
{
    public class TestCache : ICache
    {
        public Task<T> CacheAsideAsync<T>(string key, DateTime absoluteExpiration, Func<Task<T>> objectToCache)
        {
            return objectToCache();
        }
    }
}
