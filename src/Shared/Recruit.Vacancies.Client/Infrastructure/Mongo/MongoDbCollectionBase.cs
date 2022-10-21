using System;
using System.Linq;
using System.Security.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using Polly.Retry;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo
{
    public abstract class MongoDbCollectionBase
    {
        private readonly string _dbName;
        private readonly string _collectionName;
        private readonly MongoDbConnectionDetails _config;
        private readonly Lazy<ILogger> _mongoCommandLogger;
        private readonly string[] _excludedCommands = { "isMaster", "buildInfo", "saslStart", "saslContinue", "getLastError" };

        protected ILogger Logger { get; }

        protected RetryPolicy RetryPolicy { get; set; }

        protected MongoDbCollectionBase(ILoggerFactory loggerFactory, string dbName, string collectionName, IOptions<MongoDbConnectionDetails> config)
        {
            _dbName = dbName;
            _collectionName = collectionName;
            
            _config = config.Value;

            Logger = loggerFactory.CreateLogger(this.GetType().FullName);
            _mongoCommandLogger = new Lazy<ILogger>(() => loggerFactory.CreateLogger("Mongo command"));

            RetryPolicy = MongoDbRetryPolicy.GetRetryPolicy(Logger);
        }

        protected IMongoDatabase GetDatabase()
        {
            var settings = MongoClientSettings.FromUrl(new MongoUrl(_config.ConnectionString));
            settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };

            if (_config.ConnectionString.Contains("localhost:27017"))
                LogMongoCommands(settings);

            var client = new MongoClient(settings);
            var database = client.GetDatabase(_dbName);

            return database;
        }

        protected IMongoCollection<T> GetCollection<T>()
        {
            var database = GetDatabase();
            var collection = database.GetCollection<T>(_collectionName);

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
                if (_excludedCommands.Contains(e.CommandName))
                    return;

                _mongoCommandLogger.Value.LogTrace($"{e.CommandName} = {e.Command.ToJson()}");
                Console.WriteLine($"{e.CommandName} - {e.Command.ToJson()}");
            });
        }
    }
}
