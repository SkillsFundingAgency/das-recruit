using Esfa.Recruit.Storage.Client.Core.Entities;
using Esfa.Recruit.Storage.Client.Core.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Storage.Client.Core.Repositories
{
    public class MongoDbVacancyRepository : MongoDbCollectionBase, IVacancyRepository
    {

        private const string Database = "Recruit";
        private const string Collection = "Vacancy";

        public MongoDbVacancyRepository(IOptions<MongoDbConnectionDetails> details) : base(Database, Collection, details)
        {
            
        }

        public async Task<Vacancy> GetVacancyAsync(Guid id)
        {
            var filter = Builders<Vacancy>.Filter.Eq(v => v.Id, id);

            var collection = GetCollection<Vacancy>();
            var result = await collection.FindAsync(filter);
            return result.Single();
        }

        public async Task CreateVacancyAsync(Vacancy vacancy)
        {
            var collection = GetCollection<Vacancy>();

            await collection.InsertOneAsync(vacancy);
        }

        public async Task UpdateVacancyAsync(Vacancy vacancy)
        {
            var collection = GetCollection<Vacancy>();
            var filter = Builders<Vacancy>.Filter.Eq(v => v.Id, vacancy.Id);
            await collection.ReplaceOneAsync(filter, vacancy);
        }
        
        private async Task<Vacancy> FindSingleAsync(FilterDefinition<Vacancy> filter)
        {
            var collection = GetCollection<Vacancy>();
            var result = await collection.FindAsync(filter);
            return result.Single();
        }
    }
}
