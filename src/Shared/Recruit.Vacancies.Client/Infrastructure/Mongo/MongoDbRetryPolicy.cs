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
        public static RetryPolicy GetRetryPolicy(ILogger logger)
        {
            var policyBuilderJittered = 
                Policy
                    .Handle<MongoCommandException>()
                    .WaitAndRetry(
                Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(100), 5), 
                        (_, timeSpan, retryCount, _) =>
                                {
                                    logger.LogWarning($"Throttled (429). Retrying {retryCount} in {timeSpan.TotalMilliseconds}ms...");
                                });
             var policyMongo = Policy.Handle<MongoException>().WaitAndRetry(new[]
                    {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(4)
                    }, (exception, timeSpan, retryCount, context) =>
                    {
                        logger.LogWarning($"Error executing Mongo Command for method {context.OperationKey} Reason: {exception.Message}. Retrying in {timeSpan.Seconds} secs...attempt: {retryCount}");
                    });
            //var addWaitAndRetry = AddWaitAndRetry(policyBuilder, logger);
            //addWaitAndRetry.
            return policyBuilderJittered;
        }

        public static RetryPolicy GetConnectionRetryPolicy(ILogger logger)
        {
            return AddWaitAndRetry(Policy.Handle<MongoConnectionClosedException>(), logger);

        }

        private static RetryPolicy AddWaitAndRetry(PolicyBuilder policyBuilder, ILogger logger)
        {
            return policyBuilder
                .WaitAndRetry(new[]
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
