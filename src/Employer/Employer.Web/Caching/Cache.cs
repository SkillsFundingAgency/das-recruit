﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Esfa.Recruit.Employer.Web.Caching
{
    public class Cache : ICache
    {
        private readonly IMemoryCache _memoryCache;

        public Cache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<T> CacheAsideAsync<T>(string key, DateTime absoluteExpiration, Func<Task<T>> objectToCache)
        {
            return _memoryCache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpiration = absoluteExpiration;

                return objectToCache();
            });
        }
    }
}
