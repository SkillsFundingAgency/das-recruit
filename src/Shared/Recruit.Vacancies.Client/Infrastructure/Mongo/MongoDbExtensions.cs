using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo
{
    internal static class MongoDbExtensions
    {
        internal static bool Exists<T>(this IMongoCollection<T> collection)
        {
            return DetermineIfCollectionExists(collection.Database, collection.CollectionNamespace.CollectionName);
        }

        private static bool DetermineIfCollectionExists(IMongoDatabase database, string collectionName)
        {
            using (IAsyncCursor<BsonDocument> collectionCursor = database.ListCollections(new ListCollectionsOptions { Filter = Builders<BsonDocument>.Filter.Eq("name", collectionName) }))
            {
                while (collectionCursor.MoveNext())
                {
                    if (collectionCursor.Current.SingleOrDefault() != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}