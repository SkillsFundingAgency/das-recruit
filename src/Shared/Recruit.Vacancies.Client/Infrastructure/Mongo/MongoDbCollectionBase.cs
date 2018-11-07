using System;
using System.Linq;
using System.Security.Authentication;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
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
            RetryPolicy = MongoDbRetryPolicy.GetRetryPolicy(_logger);
        }

        protected IMongoCollection<T> GetCollection<T>()
        {
            var settings = MongoClientSettings.FromUrl(new MongoUrl(_config.ConnectionString));
            settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };

#if DEBUG
            //LogMongoCommands(settings);
#endif

            var client = new MongoClient(settings);
            var database = client.GetDatabase(_dbName);
            var collection = database.GetCollection<T>(_collectionName);

            if (!collection.Exists())
                throw new InfrastructureException($"Expected that collection: '{_collectionName}' would already be created.");

            return collection;
        }

        protected ProjectionDefinition<T> GetProjection<T>()
        {
            ProjectionDefinition<T> projection = null;

            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                projection = projection == null ?
                    Builders<T>.Projection.Include(propertyInfo.Name) :
                    projection.Include(propertyInfo.Name);
            }
            return projection;
        }
        
        private void LogMongoCommands(MongoClientSettings settings)
        {
            settings.ClusterConfigurator = cc => cc.Subscribe<CommandStartedEvent>(e =>
            {
                if (new[] { "isMaster", "buildInfo", "saslStart", "saslContinue", "getLastError" }.Contains(e.CommandName))
                    return;

                _logger.LogDebug($"{e.CommandName} = {e.Command.ToJson()}");
            });
        }
    }
}
