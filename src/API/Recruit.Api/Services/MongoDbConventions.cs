using MongoDB.Bson.Serialization.Conventions;

namespace SFA.DAS.Recruit.Api.Services
{
    internal static class MongoDbConventions
    {
        public static void RegisterMongoConventions()
        {
            var pack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new EnumRepresentationConvention(MongoDB.Bson.BsonType.String),
                new IgnoreExtraElementsConvention(true)
            };
            ConventionRegistry.Register("recruit conventions", pack, t => true);
        }
    }
}