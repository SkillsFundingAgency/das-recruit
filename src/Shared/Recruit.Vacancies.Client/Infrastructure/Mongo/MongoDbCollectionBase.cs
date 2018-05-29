using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using Polly.Retry;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo
{
    internal abstract class MongoDbCollectionBase
    {
        private readonly ILogger _logger;
        private readonly string _dbName;
        private readonly string _collectionName;
        private readonly MongoDbConnectionDetails _config;

        protected RetryPolicy RetryPolicy { get; }

        protected MongoDbCollectionBase(ILogger logger, string dbName, string collectionName, IOptions<MongoDbConnectionDetails> config)
        {
            _logger = logger;
            _dbName = dbName;
            _collectionName = collectionName;
            _config = config.Value;
            RetryPolicy = GetRetryPolicy();
        }

        protected IMongoCollection<T> GetCollection<T>()
        {
            var settings = MongoClientSettings.FromUrl(new MongoUrl(_config.ConnectionString));
            settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };

            var client = new MongoClient(settings);
            var database = client.GetDatabase(_dbName);
            var collection = database.GetCollection<T>(_collectionName);

            return collection;
        }

        protected Task Retry(Func<Task> func)
        {
            var policy = GetRetryPolicy();

            return policy.ExecuteAsync(func);
        }

        protected Task<T> Retry<T>(Func<Task<T>> func)
        {
            var policy = GetRetryPolicy();

            return policy.ExecuteAsync(func);
        }

        private Polly.Retry.RetryPolicy GetRetryPolicy()
        {
            return Policy
                    .Handle<MongoCommandException>()
                    .WaitAndRetryAsync(new[]
                    {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(4)
                    }, (exception, timeSpan, retryCount, context) => {
                        _logger.LogWarning($"Error executing Mongo Command for {context["methodName"]}. Retrying in {timeSpan.Seconds} secs...attempt: {retryCount}");    
                    });
        }
    }
}
