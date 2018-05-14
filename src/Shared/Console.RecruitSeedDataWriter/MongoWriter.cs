using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;

namespace Console.RecruitSeedDataWriter
{
    internal class MongoWriter
    {
        private const string KeyFieldName = "_id";
        private readonly WriterOptions _writerOptions;

        public MongoWriter(WriterOptions writerOptions)
        {
            _writerOptions = writerOptions;
        }

        internal async Task Write(BsonDocument bsonDocument, bool canOverwrite = false)
        {
            var collection = new MongoDbCollection(_writerOptions.ConnectionString, _writerOptions.CollectionName).GetCollection();

            if (canOverwrite)
            {   
                var bsonDocId = bsonDocument.Single(field => field.Name.Equals(KeyFieldName)).Value.AsString;
                var filter = Builders<BsonDocument>.Filter.Eq(KeyFieldName, bsonDocId);
                await collection.ReplaceOneAsync(filter, bsonDocument, new UpdateOptions { IsUpsert = true });
            }
            else
            {
                await collection.InsertOneAsync(bsonDocument);
            }

            await Task.CompletedTask;
        }
    }
}
