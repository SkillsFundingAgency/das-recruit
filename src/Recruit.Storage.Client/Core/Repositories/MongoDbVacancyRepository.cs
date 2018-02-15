using Esfa.Recruit.Storage.Client.Core.Entities;
using Esfa.Recruit.Storage.Client.Core.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Storage.Client.Core.Repositories
{
    public class MongoDbVacancyRepository : MongoDbCollectionBase, IVacancyRepository
    {

        private const string Database = "Recruit";
        private const string Collection = "Vacancy";

        public MongoDbVacancyRepository(IOptions<MongoDbConnectionDetails> details) : base(Database, Collection, details) { }

        public async Task<MongoVacancy> GetVacancyAsync(Guid id)
        {
            var filter = Builders<MongoVacancy>.Filter.Eq(v => v.Id, id);
            return await FindSingleAsync(filter);
        }

        public async Task CreateVacancyAsync(MongoVacancy vacancy)
        {
            var collection = GetCollection<MongoVacancy>();

            await collection.InsertOneAsync(vacancy);
        }

        public async Task UpdateVacancyAsync(MongoVacancy vacancy)
        {
            var collection = GetCollection<MongoVacancy>();
            var filter = Builders<MongoVacancy>.Filter.Eq(v => v.Id, vacancy.Id);
            await collection.ReplaceOneAsync(filter, vacancy);
        }

        public async Task<MongoVacancy> GetVacancyAsync(int vrn)
        {
            var filter = Builders<MongoVacancy>.Filter.Eq(v => v.VRN, vrn);
            return await FindSingleAsync(filter);
        }
        
        private async Task<MongoVacancy> FindSingleAsync(FilterDefinition<MongoVacancy> filter)
        {
            var collection = GetCollection<MongoVacancy>();
            var result = await collection.FindAsync(filter);
            return result.Single();
        }
    }
}
