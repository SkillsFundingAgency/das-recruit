using Esfa.Recruit.Storage.Client.Domain.Entities;
using Esfa.Recruit.Storage.Client.Domain.Repositories;
using Esfa.Recruit.Storage.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Storage.Client.Infrastructure.Repositories
{
    public class MongoDbVacancyRepository : MongoDbCollectionBase, IVacancyRepository
    {
        private const string Database = "recruit";
        private const string Collection = "vacancies";

        public MongoDbVacancyRepository(IOptions<MongoDbConnectionDetails> details) 
            : base(Database, Collection, details)
        {
        }

        public async Task CreateAsync(Vacancy vacancy)
        {
            var collection = GetCollection<Vacancy>();
            await collection.InsertOneAsync(vacancy);
        }

        public async Task<Vacancy> GetVacancyAsync(Guid id)
        {
            var filter = Builders<Vacancy>.Filter.Eq(v => v.Id, id);

            var collection = GetCollection<Vacancy>();
            var result = await collection.FindAsync(filter);
            return result.SingleOrDefault();
        }

        public async Task UpdateAsync(Vacancy vacancy)
        {
            var filter = Builders<Vacancy>.Filter.Eq("_id", vacancy.Id);
            var collection = GetCollection<Vacancy>();
            await collection.ReplaceOneAsync(filter, vacancy);
        }
    }
}
