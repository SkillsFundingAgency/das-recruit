using MongoDB.Bson.Serialization.Conventions;

namespace Esfa.Recruit.Storage.Client.Infrastructure.Mongo
{
    public static class MongoDbConventions
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
