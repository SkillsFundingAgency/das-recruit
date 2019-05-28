using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Polly;
using Polly.Retry;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.BlobStorage
{
    public class BlobStorageContainerChecker
    {
        private const string QueryStoreContainerName = "querystore";

        private readonly BlobStorageConfiguration _config;
        private readonly ILogger<BlobStorageContainerChecker> _logger;

        private RetryPolicy RetryPolicy => Policy
            .Handle<StorageException>()
            .WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(1)
            }, (exception, timeSpan, retryCount, context) =>
            {
                _logger.LogWarning($"Error executing BlobStorageContainerChecker for method {context.OperationKey} Reason: {exception.Message}. Retrying in {timeSpan.Seconds} secs...attempt: {retryCount}");
            });

        public BlobStorageContainerChecker(IOptions<BlobStorageConfiguration> config, ILogger<BlobStorageContainerChecker> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        public void EnsureBlobQueryStoreContainerExists()
        {
            _logger.LogInformation("Ensuring query store blob container has been created");

            var container = GetQueryStoreContainerReference();

            var createdContainer = RetryPolicy.ExecuteAsync(context => container.CreateIfNotExistsAsync(),
                new Context(nameof(EnsureBlobQueryStoreContainerExists))).Result;

            if (createdContainer)
            {
                _logger.LogInformation($"created blob container '{QueryStoreContainerName}'");
            }
            else
            {
                _logger.LogInformation($"blob container '{QueryStoreContainerName}' already exists");
            }
            
            _logger.LogInformation("Finished ensuring query store blob container has been created");
        }

        private CloudBlobContainer GetQueryStoreContainerReference()
        {
            var account = CloudStorageAccount.Parse(_config.ConnectionString);

            var client = account.CreateCloudBlobClient();

            var container = client.GetContainerReference(QueryStoreContainerName);

            return container;
        }
    }
}
