using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Security.Authentication;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo
{
    internal abstract class MongoDbCollectionBase
    {
        private readonly string _dbName;
        private readonly string _collectionName;
        private readonly MongoDbConnectionDetails _config;
        
        public MongoDbCollectionBase(string dbName, string collectionName, IOptions<MongoDbConnectionDetails> config)
        {
            _dbName = dbName;
            _collectionName = collectionName;
            _config = config.Value;
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
    }
}
