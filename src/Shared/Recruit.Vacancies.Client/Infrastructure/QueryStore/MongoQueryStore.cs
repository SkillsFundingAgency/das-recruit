using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    internal sealed class MongoQueryStore : MongoDbCollectionBase, IQueryStore
    {
        private const string Database = "recruit";
        private const string Collection = "queryViews";

        public MongoQueryStore(ILogger<MongoQueryStore> logger, IOptions<MongoDbConnectionDetails> details)
            : base(logger, Database, Collection, details)
        {
        }

        async Task<bool> IQueryStore.DeleteAsync<T>(string key)
        {
            var collection = GetCollection<T>();
            
            if (!collection.Exists())
                throw new InfrastructureException($"Expected that collection: {Collection} would already be created.");

            var filter = Builders<T>.Filter.Eq(d => d.Id, key);
            var result = await RetryPolicy.ExecuteAsync(() => collection.DeleteOneAsync(filter));

            return result.DeletedCount == 1;
        }

        async Task<IEnumerable<T>> IQueryStore.GetAllByTypeAsync<T>(string typeName)
        {
            var filter = Builders<T>.Filter.Eq(d => d.Type, typeName);

            var collection = GetCollection<T>();
            var result = await RetryPolicy.ExecuteAsync(() => collection.FindAsync(filter));

            return result?.ToEnumerable();
        }

        async Task<T> IQueryStore.GetAsync<T>(string key)
        {
            var filter = Builders<T>.Filter.Eq(d => d.Id, key);

            var collection = GetCollection<T>();
            var result = await RetryPolicy.ExecuteAsync(() => collection.FindAsync(filter));

            return result?.FirstOrDefault();
        }

        Task IQueryStore.UpsertAsync<T>(T item)
        {
            var collection = GetCollection<T>();
            
            if (!collection.Exists())
                throw new InfrastructureException($"Expected that collection: {Collection} would already be created.");

            var filter = Builders<T>.Filter.Eq(d => d.Id, item.Id);
            
            return RetryPolicy.ExecuteAsync(() => collection.ReplaceOneAsync(filter, item, new UpdateOptions { IsUpsert = true }));
        }
    }
}