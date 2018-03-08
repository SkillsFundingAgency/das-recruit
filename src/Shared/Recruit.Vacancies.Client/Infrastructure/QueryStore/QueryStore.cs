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

        public async Task<Dashboard> GetDashboardAsync(string viewKey)
        {
            var filter = Builders<Dashboard>.Filter.Eq(d => d.Id, viewKey);

            var collection = GetCollection<Dashboard>();
            var result = await collection.FindAsync(filter);

            return result?.FirstOrDefault();
        }

        public async Task UpdateDashboardAsync(Dashboard dashboard)
        {
            var filter = Builders<Dashboard>.Filter.Eq(d => d.Id, dashboard.Id);
            var collection = GetCollection<Dashboard>();

            await collection.ReplaceOneAsync(filter, dashboard, new UpdateOptions { IsUpsert = true });
        }

        public async Task UpdateApprenticeshipProgrammesAsync(ApprenticeshipProgrammes programmes)
        {
            var collection = GetCollection<ApprenticeshipProgrammes>();

            var filter = Builders<ApprenticeshipProgrammes>.Filter.Eq(x => x.Id, programmes.Id);
            var options = new UpdateOptions 
            {
                IsUpsert = true,
            };

            await collection.ReplaceOneAsync(filter, programmes, options);
        }
    }
}