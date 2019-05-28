using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.BlobStorage
{
    public class BlobStorageQueryStore : IQueryStore
    {
        private const string QueryStoreContainerName = "querystore";

        private readonly BlobStorageConfiguration _config;
        private readonly ILogger<BlobStorageQueryStore> _logger;

        private readonly JsonSerializerSettings _jsonWriter = new JsonSerializerSettings {
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new JsonConverterCollection() { new Newtonsoft.Json.Converters.StringEnumConverter() }
        };

        private RetryPolicy RetryPolicy => Policy
            .Handle<StorageException>(ex => ex.RequestInformation.ErrorCode != BlobErrorCodeStrings.BlobNotFound)
            .WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(1)
            }, (exception, timeSpan, retryCount, context) =>
            {
                _logger.LogWarning($"Error executing Blob Storage Command for method {context.OperationKey} Reason: {exception.Message}. Retrying in {timeSpan.Seconds} secs...attempt: {retryCount}");
            });

        public BlobStorageQueryStore(IOptions<BlobStorageConfiguration> config, ILogger<BlobStorageQueryStore> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        public async Task<long> DeleteAllAsync<T>(string typeName) where T : QueryProjectionBase
        {
            var blobs = await ListBlobsAsync(typeName);

            var deleteBlobTasks = blobs.Select(b => RetryPolicy.ExecuteAsync(context => b.DeleteAsync(),
                new Context(nameof(IQueryStore.DeleteAllAsync))));

            await Task.WhenAll(deleteBlobTasks);

            return blobs.Count;
        }

        public Task DeleteAsync<T>(string typeName, string key) where T : QueryProjectionBase
        {
            var blob = GetBlockBlobReference(typeName, key);

            return RetryPolicy.ExecuteAsync(context => blob.DeleteAsync(),
                new Context(nameof(IQueryStore.DeleteAsync)));
        }

        public async Task<long> DeleteManyLessThanAsync<T, T1>(string typeName, Expression<Func<T, T1>> property, T1 value) where T : QueryProjectionBase
        {
            var propertyInfo = GetPropertyInfo(property);

            var blobs = await ListBlobsAsync(typeName);

            IEnumerable<Task<bool>> blobTasks = blobs.Select(async b =>
            {
                var item = await GetItemFromBlobAsync<T>(b);

                var propertyValue = (IComparable)propertyInfo.GetValue(item);

                if (propertyValue.CompareTo(value) > -1)
                    return false;

                await RetryPolicy.ExecuteAsync(context => b.DeleteAsync(),
                    new Context(nameof(IQueryStore.DeleteManyLessThanAsync)));

                return true;
            }).ToList();

            await Task.WhenAll(blobTasks);

            return blobTasks.Count(t => t.Result);
        }

        public async Task<T> GetAsync<T>(string typeName, string key) where T : QueryProjectionBase
        {
            var blob = GetBlockBlobReference(typeName, key);

            var item = await GetItemFromBlobAsync<T>(blob);

            return item;
        }

        public Task UpsertAsync<T>(T item) where T : QueryProjectionBase
        {
            var blob = GetBlockBlobReference(item.ViewType, item.Id);

            var serializedItem = JsonConvert.SerializeObject(item, _jsonWriter);

            return RetryPolicy.ExecuteAsync(context => blob.UploadTextAsync(serializedItem),
                new Context(nameof(IQueryStore.UpsertAsync)));
        }

        private CloudBlockBlob GetBlockBlobReference(string typeName, string id)
        {
            var container = GetQueryStoreContainerReference();

            var blobName = $"{typeName}/{id}".ToLower();

            var blob = container.GetBlockBlobReference(blobName);

            return blob;
        }

        private CloudBlobContainer GetQueryStoreContainerReference()
        {
            var account = CloudStorageAccount.Parse(_config.ConnectionString);

            var client = account.CreateCloudBlobClient();

            var container = client.GetContainerReference(QueryStoreContainerName);

            return container;
        }

        private async Task<T> GetItemFromBlobAsync<T>(CloudBlockBlob blob)
        {
            try
            {
                var serializedItem = await RetryPolicy.ExecuteAsync(context => blob.DownloadTextAsync(),
                    new Context(nameof(GetItemFromBlobAsync)));

                var item = JsonConvert.DeserializeObject<T>(serializedItem);

                return item;
            }
            catch(StorageException ex)
            {
                if (ex.RequestInformation.ErrorCode == BlobErrorCodeStrings.BlobNotFound)
                    return default(T);

                throw;
            }
        }

        private async Task<List<CloudBlockBlob>> ListBlobsAsync(string typeName)
        {
            var blobs = new List<CloudBlockBlob>();
            BlobContinuationToken continuationToken = null;

            var directoryRelativeAddress = typeName.ToLower();
            var container = GetQueryStoreContainerReference();
            var directory = container.GetDirectoryReference(directoryRelativeAddress);

            do
            {
                var response = await directory.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = response.ContinuationToken;

                foreach (var result in response.Results)
                {
                    if (result is CloudBlockBlob blob)
                        blobs.Add(blob);
                }
            }
            while (continuationToken != null);

            return blobs;
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
