using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.TableStore
{
    internal sealed class TableStorageQueryStore : TableStorageBase, IQueryStore
    {
        public CloudTableClient TableClient { get; }
        public CloudTable CloudTable { get; }

        public TableStorageQueryStore(ILoggerFactory loggerFactory, IOptions<TableStorageConnectionsDetails> details)
            : base(loggerFactory, details)
        {
            var storageAccount = CloudStorageAccount.Parse(details.Value.ConnectionString);
            TableClient = storageAccount.CreateCloudTableClient();
            CloudTable = TableClient.GetTableReference(TableStorageTblNames.TableName);
        }

        public async Task<T> GetAsync<T>(string typeName, string key) where T : QueryProjectionBase
        {
            var retrieveOperation = TableOperation.Retrieve<QueryEntity>(typeName, key);
            var result = await CloudTable.ExecuteAsync(retrieveOperation);
            var queryEntity = (QueryEntity)result.Result;
            if (queryEntity != null)
            {
                var actualItem = JsonConvert.DeserializeObject<T>(queryEntity.JsonData);
                return actualItem;
            }
            return null;
        }

        public Task UpsertAsync<T>(T item) where T : QueryProjectionBase
        {
            CheckIfTableExists();
            var serializedItem = JsonConvert.SerializeObject(item);
            var query = new QueryEntity(item.ViewType, item.Id) {
                JsonData = serializedItem
            };
            var insertOrReplaceOperation = TableOperation.InsertOrReplace(query);
            var retrievedResult = CloudTable.ExecuteAsync(insertOrReplaceOperation);
            return retrievedResult;
        }

        public async Task RecreateAsync<T>(string typeName, IList<T> items) where T : QueryProjectionBase
        {
            var batchDeleteOperation = new TableBatchOperation();
            var batchInsertOperation = new TableBatchOperation();
            var query = new TableQuery<QueryEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, typeName));
            var retrievedResult = await CloudTable.ExecuteQuerySegmentedAsync(query, null);
            if (retrievedResult.Results.Any())
            {
                foreach (var queryEntity in retrievedResult)
                {
                    batchDeleteOperation.Delete(queryEntity);
                }
                await CloudTable.ExecuteBatchAsync(batchDeleteOperation);
            }
            if (items.Count == 0) return;
            foreach (var item in items)
            {
                var serializedItem = JsonConvert.SerializeObject(item);
                var entity = new QueryEntity(item.ViewType, item.Id) {
                    JsonData = serializedItem
                };
                batchInsertOperation.Insert(entity);
            }
            await CloudTable.ExecuteBatchAsync(batchInsertOperation);
        }

        public async Task DeleteAsync<T>(string typeName, string key) where T : QueryProjectionBase
        {
            var retrieveOperation = TableOperation.Retrieve<QueryEntity>(typeName, key);
            var retrievedResult = await CloudTable.ExecuteAsync(retrieveOperation);
            var deleteEntity = (QueryEntity)retrievedResult.Result;
            if (deleteEntity != null)
            {
                var tableOperation = TableOperation.Delete(deleteEntity);
                await CloudTable.ExecuteAsync(tableOperation);
            }
        }

        public async Task<long> DeleteManyAsync<T, T1>(string typeName, Expression<Func<T, T1>> property, T1 value) where T : QueryProjectionBase
        {
            var propertyInfo = GetPropertyInfo(property);
            var rangeQuery = new TableQuery<QueryEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, typeName),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition(propertyInfo.Name, QueryComparisons.LessThan, value.ToString())));
            var retrievedResults = await CloudTable.ExecuteQuerySegmentedAsync(rangeQuery, null);
            return retrievedResults.Results.Count;
        }

        public Task<bool> CheckIfTableExists()
        {
            return CloudTable.CreateIfNotExistsAsync();
        }

        private PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            var member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.");

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.");
            return propInfo;
        }
    }
}
