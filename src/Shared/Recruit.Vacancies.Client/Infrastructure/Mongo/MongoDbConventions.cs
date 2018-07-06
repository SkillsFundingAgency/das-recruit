using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo
{
    internal static class MongoDbConventions
    {
        public static void RegisterMongoConventions()
        {
            var pack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new EnumRepresentationConvention(MongoDB.Bson.BsonType.String),
                new IgnoreExtraElementsConvention(true),
                new IgnoreIfNullConvention(true)
            };
            ConventionRegistry.Register("recruit conventions", pack, t => true);

            BsonClassMap.RegisterClassMap<Vacancy>(cm => {
                cm.AutoMap();
                cm.GetMemberMap(c => c.HasCompletedPart1).SetDefaultValue(true);
            });
        }
    }
}
