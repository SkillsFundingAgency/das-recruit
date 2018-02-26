using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    internal sealed class QueryStore : MongoDbCollectionBase, IQueryStoreReader, IQueryStoreWriter
    {
        private const string Database = "query-store";
        private const string Collection = "dashboard-views";

        public QueryStore(IOptions<MongoDbConnectionDetails> details)
            : base(Database, Collection, details)
        {
        }

        public async Task<Dashboard> GetDashboardAsync(string key)
        {
            var filter = Builders<Dashboard>.Filter.Eq(d => d.EmployerAccountId, key);

            var collection = GetCollection<Dashboard>();
            var result = await collection.FindAsync(filter);

            return result?.FirstOrDefault();
        }

        public async Task UpdateDashboardAsync(string key, Dashboard dashboard)
        {
            //var changesDocument = dashboard.ToBsonDocument();

            //var update = new BsonDocumentUpdateDefinition<BsonDocument>(new BsonDocument("$set", changesDocument));

            var filter = Builders<Dashboard>.Filter.Eq(d => d.EmployerAccountId, key);
            var collection = GetCollection<Dashboard>();
            //collection.UpdateOne<Dashboard>(filter, new UpdateDefinition<Dashboard>())

            await collection.ReplaceOneAsync(filter, dashboard, new UpdateOptions { IsUpsert = true });
        }
    }
}