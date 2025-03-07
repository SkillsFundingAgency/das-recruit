using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Polly;
using Polly.Retry;
using Polly.Contrib.WaitAndRetry;
using System;
using Polly.Wrap;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo
{
    public static class MongoDbRetryPolicy
    {
        public static AsyncRetryPolicy GetRetryPolicy(ILogger logger)
        {
            var policyBuilderJittered = 
                Policy
                    .Handle<MongoCommandException>()
                    .WaitAndRetryAsync(
                Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(100), 5), 
                        (_, timeSpan, retryCount, context) =>
                                {
                                    logger.LogWarning($"Throttled (429) {context.OperationKey}. Retrying {retryCount} in {timeSpan.TotalMilliseconds}ms...");
                                });
            
            return policyBuilderJittered;
        }

        public static AsyncRetryPolicy GetConnectionRetryPolicy(ILogger logger)
        {
            return AddWaitAndRetry(Policy.Handle<MongoConnectionClosedException>(), logger);

        }

        private static AsyncRetryPolicy AddWaitAndRetry(PolicyBuilder policyBuilder, ILogger logger)
        {
            return policyBuilder
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(4)
                }, (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogWarning($"Error executing Mongo Command for method {context.OperationKey} Reason: {exception.Message}. Retrying in {timeSpan.Seconds} secs...attempt: {retryCount}");
                });
        }
    }
}
