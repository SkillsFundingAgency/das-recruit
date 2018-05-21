using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Console.RecruitSeedDataWriter
{
    internal class MongoWriter
    {
        private const string KeyFieldName = "_id";
        private readonly WriterOptions _writerOptions;

        public MongoWriter(WriterOptions writerOptions)
        {
            _writerOptions = writerOptions;
        }

        internal async Task<WriteOperationResult> Write(BsonDocument bsonDocument, bool canOverwrite)
        {
            var collection = await new MongoDbCollection(_writerOptions.ConnectionString, _writerOptions.CollectionName).GetCollection();

            var bsonDocId = bsonDocument.Single(field => field.Name.Equals(KeyFieldName)).Value.AsString;
            var filter = Builders<BsonDocument>.Filter.Eq(KeyFieldName, bsonDocId);

            if (canOverwrite)
            {
                var replaceResult = await collection.ReplaceOneAsync(filter, bsonDocument, new UpdateOptions { IsUpsert = true });
                return  replaceResult.UpsertedId != null 
                        ? WriteOperationResult.Inserted
                        : WriteOperationResult.Replaced;
            }
            else
            {
                var options = new FindOptions<BsonDocument>
                {
                    Limit = 1
                };

                var existingMatchingDocuments = await collection.FindAsync(filter, options);

                if (existingMatchingDocuments.Any() == false)
                {
                    await collection.InsertOneAsync(bsonDocument);
                    return WriteOperationResult.Inserted;
                }

                return WriteOperationResult.Skipped;
            }
        }
    }
}
