using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Authentication;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Console.RecruitSeedDataWriter
{
    internal class MongoDbCollection
    {
        private const string BsonCollectionDocumentNameField = "name";
        private readonly string _collectionName;
        private readonly MongoUrl _connString;

        public MongoDbCollection(MongoUrl connString, string collectionName)
        {
            _connString = connString;
            _collectionName = collectionName;
        }

        public async Task<IMongoCollection<BsonDocument>> GetCollection()
        {
            var settings = MongoClientSettings.FromUrl(_connString);
            settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };

            var client = new MongoClient(settings);
            var database = client.GetDatabase(_connString.DatabaseName);

            if (!(await database.ListCollectionsAsync()).ToEnumerable().Any(coll => CollectionNameExists(coll, _collectionName)))
            {
                throw new Exception($"Collection {_collectionName} does not exist in database {_connString.DatabaseName}");
            }

            var collection = database.GetCollection<BsonDocument>(_collectionName);

            return collection;
        }

        private bool CollectionNameExists(BsonDocument bsonDoc, string collectionName)
            => bsonDoc.Single(field => field.Name.Equals(BsonCollectionDocumentNameField)).Value.Equals(collectionName);
    }
}
