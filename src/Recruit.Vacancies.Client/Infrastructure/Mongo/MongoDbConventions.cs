using MongoDB.Bson.Serialization.Conventions;

namespace Esfa.Recruit.Storage.Client.Infrastructure.Mongo
{
    internal static class MongoDbConventions
    {
        public static void RegisterMongoConventions()
        {
            var pack = new ConventionPack
            {
                new CamelCaseElementNameConvention()
            };
            ConventionRegistry.Register("camelCase", pack, t => true);
        }
    }
}
