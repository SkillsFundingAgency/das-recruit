using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    internal sealed class MongoDbVacancyRepository : MongoDbCollectionBase, IVacancyRepository
    {
        private const string Database = "recruit";
        private const string Collection = "vacancies";

        private const string EmployerAccountId = "employerAccountId";
        private const string IsDeleted = "isDeleted";

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

        public async Task<IEnumerable<T>> GetVacanciesByEmployerAccountAsync<T>(string employerAccountId)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(EmployerAccountId, employerAccountId);

            var collection = GetCollection<BsonDocument>();
            var result = await collection.FindAsync<T>(filter);
    
            return await result.ToListAsync();
        }

        public async Task UpdateAsync(Vacancy vacancy)
        {
            var filter = Builders<Vacancy>.Filter.Eq(v => v.Id, vacancy.Id);
            var collection = GetCollection<Vacancy>();
            await collection.ReplaceOneAsync(filter, vacancy);
        }
    }
}
