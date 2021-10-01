using System;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Polly;
using Polly.Retry;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.TableStore
{
    public static class StorageRetryPolicy
    {
        public static RetryPolicy GetRetryPolicy(ILogger logger)
        {
            return Policy
                .Handle<StorageException>()
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1)
                }, (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogWarning($"Error executing Storage Command for method {context.OperationKey} Reason: {exception.Message}. Retrying in {timeSpan.Seconds} secs...attempt: {retryCount}");
                });
        }
    }
}
