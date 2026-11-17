using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Esfa.Recruit.Vacancies.Client.Application.Cache;

public class Cache(IMemoryCache memoryCache) : ICache
{
    public Task<T> CacheAsideAsync<T>(string key, DateTime absoluteExpiration, Func<Task<T>> objectToCache)
    {
        return memoryCache.GetOrCreateAsync(key, entry =>
        {
            entry.AbsoluteExpiration = absoluteExpiration;

            return objectToCache();
        });
    }
}