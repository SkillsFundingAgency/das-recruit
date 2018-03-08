using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    internal sealed class MongoDbVacancyRepository : MongoDbCollectionBase, IVacancyRepository
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
            filter = filter & Builders<Vacancy>.Filter.Eq(v => v.IsDeleted, false);

            var collection = GetCollection<Vacancy>();
            var result = await collection.FindAsync(filter);
            return result.SingleOrDefault();
        }

        public async Task<IEnumerable<Vacancy>> GetVacanciesByEmployerAccountAsync(string employerAccountId)
        {
            var filter = Builders<Vacancy>.Filter.Eq(v => v.EmployerAccountId, employerAccountId);
            filter = filter & Builders<Vacancy>.Filter.Eq(v => v.IsDeleted, false);

            var collection = GetCollection<Vacancy>();
            var result = await collection.FindAsync(filter);
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
