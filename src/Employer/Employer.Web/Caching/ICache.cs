using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Caching
{
    public interface ICache
    {
        Task<T> CacheAsideAsync<T>(string key, DateTime absoluteExpiration, Func<Task<T>> objectToCache);
    }
}
