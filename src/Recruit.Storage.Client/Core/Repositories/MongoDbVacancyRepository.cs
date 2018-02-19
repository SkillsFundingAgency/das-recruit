using Esfa.Recruit.Storage.Client.Core.Entities;
using Esfa.Recruit.Storage.Client.Core.Entities.VacancyPatches;
using Esfa.Recruit.Storage.Client.Core.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Storage.Client.Core.Repositories
{
    public class MongoDbVacancyRepository : MongoDbCollectionBase, ICommandVacancyRepository, IQueryVacancyRepository
    {

        private const string Database = "Recruit";
        private const string Collection = "Vacancy";

        public MongoDbVacancyRepository(IOptions<MongoDbConnectionDetails> details) 
            : base(Database, Collection, details)
        {
        }

        public async Task<Vacancy> GetVacancyAsync(Guid vacancyId)
        {
            var filter = Builders<Vacancy>.Filter.Eq(v => v.Id, vacancyId);

            var collection = GetCollection<Vacancy>();
            var result = await collection.FindAsync(filter);
            return result.Single();
        }

        public async Task UpsertVacancyAsync(Guid vacancyId, IVacancyPatch patch)
        {
            var changesDocument = patch.ToBsonDocument();

            //We don't want to change the type so remove the discriminator value from the update
            changesDocument.Remove("_t");

            var update = new BsonDocumentUpdateDefinition<BsonDocument>(new BsonDocument("$set", changesDocument));

            var filter = Builders<BsonDocument>.Filter.Eq("_id", vacancyId);
            var collection = GetCollection<BsonDocument>();
            var result = await collection.UpdateOneAsync(filter, update, new UpdateOptions {IsUpsert = true } );
        }
    }
}
