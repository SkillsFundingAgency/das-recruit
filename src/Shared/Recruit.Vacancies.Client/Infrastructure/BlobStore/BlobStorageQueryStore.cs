using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Esfa.Recruit.Vacancies.Client.Infrastructure.TableStore;
using Polly.Retry;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.BlobStore
{
    public class BlobStorageQueryStore : IQueryStore
    {
        private const int DeleteBatchSize = 100;
        //private const string PartitionKeyFieldName = nameof(TableEntity.PartitionKey);
        private readonly JsonSerializerSettings _jsonWriter = new JsonSerializerSettings
        {            
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new JsonConverterCollection() { new Newtonsoft.Json.Converters.StringEnumConverter() }
        };
        private readonly ILogger<BlobStorageQueryStore> _logger;

        private Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer Blob { get; }
        private RetryPolicy RetryPolicy { get; }

        public BlobStorageQueryStore(ILogger<BlobStorageQueryStore> logger, IOptions<TableStorageConnectionsDetails> details)
        {
            _logger = logger;
            var storageAccount = CloudStorageAccount.Parse(details.Value.ConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            Blob = blobClient.GetContainerReference(StorageTableNames.QueryStore);
            
            RetryPolicy = StorageRetryPolicy.GetRetryPolicy(logger);
        }

        public Task<long> DeleteAllAsync<T>(string typeName) where T : QueryProjectionBase
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync<T>(string typeName, string key) where T : QueryProjectionBase
        {
            throw new NotImplementedException();
        }

        public Task<long> DeleteManyAsync<T, T1>(string typeName, Expression<Func<T, T1>> property, T1 value) where T : QueryProjectionBase
        {
            throw new NotImplementedException();
        }

        public async Task<T> GetAsync<T>(string typeName, string key) where T : QueryProjectionBase
        {
            await Blob.CreateIfNotExistsAsync();
            var x = Blob.GetBlockBlobReference("x");
            await x.UploadTextAsync("x");

            throw new NotImplementedException();
        }

        public Task UpsertAsync<T>(T item) where T : QueryProjectionBase
        {
            throw new NotImplementedException();
        }
    }
}