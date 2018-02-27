using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using Esfa.Recruit.Vacancies.Client.Domain.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    internal sealed class QueryStore : MongoDbCollectionBase, IQueryStoreReader, IQueryStoreWriter
    {
        private const string Database = "recruit";
        private const string Collection = "queryViews";

        public QueryStore(IOptions<MongoDbConnectionDetails> details)
            : base(Database, Collection, details)
        {
        }

        public async Task<Dashboard> GetDashboardAsync(string key)
        {
            var filter = Builders<Dashboard>.Filter.Eq(d => d.ViewKey, key);

            var collection = GetCollection<Dashboard>();
            var result = await collection.FindAsync(filter);

            return result?.FirstOrDefault();
        }

        public async Task UpdateDashboardAsync(string key, Dashboard dashboard)
        {
            var filter = Builders<Dashboard>.Filter.Eq(d => d.ViewKey, key);
            var collection = GetCollection<Dashboard>();
            var result = await collection.FindAsync(filter);

            var existingDashboard = result?.FirstOrDefault();

            if (existingDashboard != null)
            {
                dashboard.Id = existingDashboard.Id;
            }

            await collection.ReplaceOneAsync(filter, dashboard, new UpdateOptions { IsUpsert = true });
        }
    }
}