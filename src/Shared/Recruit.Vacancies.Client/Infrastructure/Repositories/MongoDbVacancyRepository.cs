using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    internal sealed class MongoDbVacancyRepository : MongoDbCollectionBase, IVacancyRepository
    {
        private const string Database = "recruit";
        private const string Collection = "vacancies";

        private const string EmployerAccountId = "employerAccountId";

        public MongoDbVacancyRepository(ILogger<MongoDbVacancyRepository> logger, IOptions<MongoDbConnectionDetails> details) 
            : base(logger, Database, Collection, details)
        {
        }

        public async Task CreateAsync(Vacancy vacancy)
        {
            var collection = GetCollection<Vacancy>();
            await RetryPolicy.ExecuteAsync(() => collection.InsertOneAsync(vacancy));
        }

        public async Task<Vacancy> GetVacancyAsync(long vacancyReference)
        {
            var vacancy = await RetryPolicy.ExecuteAsync(() => FindVacancy(v => v.VacancyReference, vacancyReference));

            if (vacancy == null)
                throw new VacancyNotFoundException(string.Format(ExceptionMessages.VacancyWithReferenceNotFound, vacancyReference));

            return vacancy;
        }

        public async Task<Vacancy> GetVacancyAsync(Guid id)
        {
            var vacancy = await RetryPolicy.ExecuteAsync(() => FindVacancy(v => v.Id, id));

            if (vacancy == null)
                throw new VacancyNotFoundException(string.Format(ExceptionMessages.VacancyWithIdNotFound, id));

            return vacancy;
        }

        private async Task<Vacancy> FindVacancy<TField>(Expression<Func<Vacancy, TField>> expression, TField value)
        {
            var filter = Builders<Vacancy>.Filter.Eq(expression, value);
            var options = new FindOptions<Vacancy> { Limit = 1 };

            var collection = GetCollection<Vacancy>();
            var result = await RetryPolicy.ExecuteAsync(() => collection.FindAsync(filter, options));
            return result.SingleOrDefault();
        }

        public async Task<IEnumerable<T>> GetVacanciesByEmployerAccountAsync<T>(string employerAccountId)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(EmployerAccountId, employerAccountId);

            var collection = GetCollection<BsonDocument>();

            var result = await RetryPolicy.ExecuteAsync(() => collection.FindAsync<T>(filter));
    
            return await result.ToListAsync();
        }

        public async Task<Vacancy> GetSingleVacancyForPostcode(string postcode)
        {
            var builder = Builders<Vacancy>.Filter;
            var filter = builder.Eq(v => v.EmployerLocation.Postcode, postcode) &
                         builder.Ne(v => v.EmployerLocation.Latitude, null) &
                         builder.Ne(v => v.EmployerLocation.Longitude, null);

            var options = new FindOptions<Vacancy>
            {
                Sort = Builders<Vacancy>.Sort.Descending(v => v.VacancyReference),
                Limit = 1
            };

            var collection = GetCollection<Vacancy>();
            var result = await RetryPolicy.ExecuteAsync(() => collection.FindAsync(filter, options));
            return result.SingleOrDefault();
        }

        public async Task UpdateAsync(Vacancy vacancy)
        {
            var filter = Builders<Vacancy>.Filter.Eq(v => v.Id, vacancy.Id);
            var collection = GetCollection<Vacancy>();
            await RetryPolicy.ExecuteAsync(() => collection.ReplaceOneAsync(filter, vacancy));
        }
    }
}
