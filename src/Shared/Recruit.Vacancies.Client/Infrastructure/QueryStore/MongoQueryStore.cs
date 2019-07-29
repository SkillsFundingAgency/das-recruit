using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    internal sealed class MongoQueryStore : MongoDbCollectionBase, IQueryStore
    {
        public MongoQueryStore(ILoggerFactory loggerFactory, IOptions<MongoDbConnectionDetails> details)
            : base(loggerFactory, MongoDbNames.RecruitDb, MongoDbCollectionNames.QueryStore, details)
        {
        }

        Task IQueryStore.DeleteAsync<T>(string typeName, string key)
        {
            var collection = GetCollection<T>();

            var filterBuilder = Builders<T>.Filter;

            var filter = filterBuilder.Eq(d => d.ViewType, typeName)
                        & filterBuilder.Eq(d => d.Id, key);

            return RetryPolicy.ExecuteAsync(_ =>
                collection.DeleteOneAsync(filter),
                new Context(nameof(IQueryStore.DeleteAsync)));
        }

        public async Task<long> DeleteManyLessThanAsync<T, T1>(string typeName, Expression<Func<T, T1>> property, T1 value) where T : QueryProjectionBase
        {
            var filterBuilder = Builders<T>.Filter;

            var filter = filterBuilder.Eq(d => d.ViewType, typeName)
                        & filterBuilder.Lt(property, value);

            var collection = GetCollection<T>();

            var result = await RetryPolicy.ExecuteAsync(_ =>
                collection.DeleteManyAsync(filter),
                new Context(nameof(IQueryStore.DeleteManyLessThanAsync)));

            return result.DeletedCount;
        }

        public async Task<long> DeleteAllAsync<T>(string typeName) where T : QueryProjectionBase
        {
            var filterBuilder = Builders<T>.Filter;

            var filter = filterBuilder.Eq(d => d.ViewType, typeName);

            var collection = GetCollection<T>();

            var result = await RetryPolicy.ExecuteAsync(_ =>
                collection.DeleteManyAsync(filter),
                new Context(nameof(IQueryStore.DeleteManyLessThanAsync)));

            return result.DeletedCount;
        }

        async Task<T> IQueryStore.GetAsync<T>(string typeName, string key)
        {
            var filterBuilder = Builders<T>.Filter;

            var filter = filterBuilder.Eq(d => d.ViewType, typeName)
                        & filterBuilder.Eq(d => d.Id, key);

            var collection = GetCollection<T>();
            var result = await RetryPolicy.ExecuteAsync(_ =>
                collection.Find(filter).FirstOrDefaultAsync(),
                new Context(nameof(IQueryStore.GetAsync)));

            return result;
        }

        async Task<T> IQueryStore.GetAsync<T>(string key)
        {
            var filterBuilder = Builders<T>.Filter;

            var filter = filterBuilder.Eq(d => d.Id, key);

            var collection = GetCollection<T>();
            var result = await RetryPolicy.ExecuteAsync(_ =>
                collection.Find(filter).FirstOrDefaultAsync(),
                new Context(nameof(IQueryStore.GetAsync)));

            return result;
        }

        Task IQueryStore.UpsertAsync<T>(T item)
        {
            var collection = GetCollection<T>();

            var filterBuilder = Builders<T>.Filter;

            var filter = filterBuilder.Eq(d => d.ViewType, item.ViewType)
                        & filterBuilder.Eq(d => d.Id, item.Id);

            return RetryPolicy.ExecuteAsync(_ =>
                collection.ReplaceOneAsync(filter, item, new UpdateOptions { IsUpsert = true }),
                new Context(nameof(IQueryStore.UpsertAsync)));
        }
    }
}