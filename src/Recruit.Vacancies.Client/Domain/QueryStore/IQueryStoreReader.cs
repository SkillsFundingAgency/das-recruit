using System;
using System.Threading.Tasks;
using Esfa.Recruit.Storage.Client.Domain.Entities;

namespace Esfa.Recruit.Storage.Client.Domain.QueryStore
{
    public interface IQueryStoreReader
    {
        // E.g. Task<object> GetDashboardAsync(string key);
    }
}