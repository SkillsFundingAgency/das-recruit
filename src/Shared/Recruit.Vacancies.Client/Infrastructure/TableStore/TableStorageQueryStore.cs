using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using RestSharp.Extensions;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.TableStore
{
    internal sealed class TableStorageQueryStore : TableStorageBase, IQueryStore
    {
        private readonly CloudStorageAccount _storageAccount;
        public CloudTableClient TableClient { get; }
        public CloudTable CloudTable { get; }

        public TableStorageQueryStore(ILoggerFactory loggerFactory, IOptions<TableStorageConnectionsDetails> details)
            : base(loggerFactory, details)
        {
            _storageAccount = CloudStorageAccount.Parse(details.Value.ConnectionString);
            TableClient = _storageAccount.CreateCloudTableClient();
            CloudTable = TableClient.GetTableReference(TableStorageTblNames.TableName);
        }

        public Task<IEnumerable<T>> GetAllByTypeAsync<T>(string typeName) where T : QueryProjectionBase
        {
            TableQuery<QueryEntity> query = new TableQuery<QueryEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, typeName));
            return new Task<IEnumerable<T>>(null);
        }

        public async Task<T> GetAsync<T>(string typeName, string key) where T : QueryProjectionBase
        {
            var retrieveOperation = TableOperation.Retrieve<QueryEntity>(typeName, key);
            // Execute the retrieve operation.
            var result = await CloudTable.ExecuteAsync(retrieveOperation);
            var queryEntity = (QueryEntity)result.Result;
            var actualItem = JsonConvert.DeserializeObject<T>(queryEntity.JsonData);
            return actualItem;
        }

        public Task UpsertAsync<T>(T item) where T : QueryProjectionBase
        {
            CheckIfTableExists();
            var serializedItem = JsonConvert.SerializeObject((item));
            var query = new QueryEntity(item.ViewType, item.Id) {
                JsonData = serializedItem
            };
            var insertOrReplaceOperation = TableOperation.InsertOrReplace(query);
            var retrievedResult = CloudTable.ExecuteAsync(insertOrReplaceOperation);
            return retrievedResult;
        }

        public Task RecreateAsync<T>(string typeName, IList<T> items) where T : QueryProjectionBase
        {
            CheckIfTableExists();
            return null;
        }

        public Task DeleteAsync<T>(string typeName, string key) where T : QueryProjectionBase
        {
            return null;
        }

        public Task<long> DeleteManyAsync<T, T1>(string typeName, Expression<Func<T, T1>> property, T1 value) where T : QueryProjectionBase
        {
            return null;
        }

        public Task<bool> CheckIfTableExists()
        {
            return CloudTable.CreateIfNotExistsAsync();
        }
    }
}
