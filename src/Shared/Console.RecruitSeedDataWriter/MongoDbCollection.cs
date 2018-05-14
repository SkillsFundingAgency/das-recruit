using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Authentication;

namespace Console.RecruitSeedDataWriter
{
    internal class MongoDbCollection
    {
        private readonly string _collectionName;
        private readonly MongoUrl _connString;

        public MongoDbCollection(MongoUrl connString, string collectionName)
        {
            _connString = connString;
            _collectionName = collectionName;
        }

        public IMongoCollection<BsonDocument> GetCollection()
        {
            var settings = MongoClientSettings.FromUrl(_connString);
            settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };

            var client = new MongoClient(settings);
            var database = client.GetDatabase(_connString.DatabaseName);
            var collection = database.GetCollection<BsonDocument>(_collectionName);

            return collection;
        }
    }
}
