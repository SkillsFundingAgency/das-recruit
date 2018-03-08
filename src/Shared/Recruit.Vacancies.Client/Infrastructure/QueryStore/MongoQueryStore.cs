using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    internal sealed class MongoQueryStore : MongoDbCollectionBase, IQueryStore
    {
        private const string Database = "recruit";
        private const string Collection = "queryViews";

        public MongoQueryStore(IOptions<MongoDbConnectionDetails> details)
            : base(Database, Collection, details)
        {
        }

        async Task<T> IQueryStore.GetAsync<T>(string key)
        {
            var filter = Builders<T>.Filter.Eq(d => d.Id, key);

            var collection = GetCollection<T>();
            var result = await collection.FindAsync(filter);

            return result?.FirstOrDefault();
        }

        Task IQueryStore.UpdsertAsync<T>(T item)
        {
            var filter = Builders<T>.Filter.Eq(d => d.Id, item.Id);
            var collection = GetCollection<T>();

            return collection.ReplaceOneAsync(filter, item, new UpdateOptions { IsUpsert = true });
        }
    }
}