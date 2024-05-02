using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.TableStore
{
    internal sealed class TableStorageQueryStore : IQueryStore
    {
        private const int DeleteBatchSize = 100;
        private const string PartitionKeyFieldName = nameof(TableEntity.PartitionKey);
        private readonly JsonSerializerSettings _jsonWriter = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new JsonConverterCollection() { new Newtonsoft.Json.Converters.StringEnumConverter() }
        };
        private readonly ILogger<TableStorageQueryStore> _logger;
        private CloudTable CloudTable { get; }
        private RetryPolicy RetryPolicy { get; }

        public TableStorageQueryStore(ILogger<TableStorageQueryStore> logger, IOptions<TableStorageConnectionsDetails> details)
        {
            _logger = logger;
            var storageAccount = CloudStorageAccount.Parse(details.Value.ConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            CloudTable = tableClient.GetTableReference(StorageTableNames.QueryStore);
            RetryPolicy = StorageRetryPolicy.GetRetryPolicy(logger);
        }

        public async Task<T> GetAsync<T>(string typeName, string key) where T : QueryProjectionBase
        {
            var retrieveOperation = TableOperation.Retrieve<QueryEntity>(typeName, key);
            var result = await RetryPolicy.Execute(context => CloudTable.ExecuteAsync(retrieveOperation), new Context(nameof(IQueryStore.GetAsync)));
            var queryEntity = (QueryEntity)result.Result;

            if (queryEntity != null)
            {
                var actualItem = JsonConvert.DeserializeObject<T>(queryEntity.JsonData);
                return actualItem;
            }

            return null;
        }

        public async Task<T> GetAsync<T>(string key) where T : QueryProjectionBase
        {
            var retrieveOperation = TableOperation.Retrieve<QueryEntity>(key, key);
            var result = await RetryPolicy.Execute(context => CloudTable.ExecuteAsync(retrieveOperation), new Context(nameof(IQueryStore.GetAsync)));
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
            var serializedItem = JsonConvert.SerializeObject(item, _jsonWriter);
            var query = new QueryEntity(item.ViewType, item.Id, serializedItem);
            var insertOrReplaceOperation = TableOperation.InsertOrReplace(query);
            var retrievedResult = RetryPolicy.Execute(context => CloudTable.ExecuteAsync(insertOrReplaceOperation), new Context(nameof(IQueryStore.UpsertAsync)));
            return retrievedResult;
        }

        public Task<long> DeleteAllAsync<T>(string typeName) where T : QueryProjectionBase
        {
            var query = new TableQuery<QueryEntity>().Where(
                TableQuery.GenerateFilterCondition(PartitionKeyFieldName, QueryComparisons.Equal, typeName));

            return DeleteInBatchesAsync(query);
        }

        public Task<IEnumerable<LiveVacancy>> GetAllLiveExpired(DateTime? closingDate)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LiveVacancy>> GetAllLiveVacancies(int vacanciesToSkip, int vacanciesToGet)
        {
            throw new NotImplementedException();
        }
        public Task<long> GetAllLiveVacanciesCount()
        {
            throw new NotImplementedException();
        }

        public Task<LiveVacancy> GetLiveVacancy(long vacancyReference)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ClosedVacancy>> GetClosedVacancies(IList<long> vacancyReferences)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync<T>(string typeName, string key) where T : QueryProjectionBase
        {
            var retrieveOperation = TableOperation.Retrieve<QueryEntity>(typeName, key);
            var retrievedResult = await RetryPolicy.Execute(async context => await CloudTable.ExecuteAsync(retrieveOperation), new Context(nameof(IQueryStore.DeleteAsync)));
            var deleteEntity = (QueryEntity)retrievedResult.Result;
            if (deleteEntity != null)
            {
                var tableOperation = TableOperation.Delete(deleteEntity);
                await RetryPolicy.Execute(async context => await CloudTable.ExecuteAsync(tableOperation), new Context(nameof(IQueryStore.DeleteAsync)));
                _logger.LogInformation($"Entity deleted with typeName:{typeName} and key:{key}");
            }
        }

        public Task<long> DeleteManyLessThanAsync<T, T1>(string typeName, Expression<Func<T, T1>> property, T1 value) where T : QueryProjectionBase
        {
            var propertyInfo = GetPropertyInfo(property);
            var rangeQuery = new TableQuery<QueryEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition(PartitionKeyFieldName, QueryComparisons.Equal, typeName),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition(propertyInfo.Name, QueryComparisons.LessThan, value.ToString())));

            return DeleteInBatchesAsync(rangeQuery);
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

        private async Task<long> DeleteInBatchesAsync(TableQuery<QueryEntity> query)
        {
            long deletedCount = 0;

            TableQuerySegment<QueryEntity> segment = null;

            while (segment == null || segment.ContinuationToken != null)
            {
                segment = await RetryPolicy.Execute(_ =>
                    CloudTable.ExecuteQuerySegmentedAsync(query.Take(DeleteBatchSize),
                    segment?.ContinuationToken),
                    new Context($"{nameof(DeleteInBatchesAsync)}-ExecuteQuerySegmentedAsync"));

                var batch = new TableBatchOperation();

                foreach (var entity in segment.Results)
                {
                    batch.Add(TableOperation.Delete(entity));
                }

                if (batch.Count == 0)
                    continue;

                var result = await RetryPolicy.Execute(_ =>
                        CloudTable.ExecuteBatchAsync(batch),
                    new Context($"{nameof(DeleteInBatchesAsync)}-ExecuteBatchAsync"));

                deletedCount += result.Count;
            }

            return deletedCount;
        }
    }
}
