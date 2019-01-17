using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Linq.Expressions;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;
using System.Linq;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    internal sealed class MongoDbVacancyRepository : MongoDbCollectionBase, IVacancyRepository, IVacancyQuery
    {
        private const string EmployerAccountIdFieldName = "employerAccountId";
        private const string OwnerTypeFieldName = "ownerType";
        private const string IsDeletedFieldName = "isDeleted";

        public MongoDbVacancyRepository(ILoggerFactory loggerFactory, IOptions<MongoDbConnectionDetails> details) 
            : base(loggerFactory, MongoDbNames.RecruitDb, MongoDbCollectionNames.Vacancies, details)
        {
        }

        public async Task CreateAsync(Vacancy vacancy)
        {
            var collection = GetCollection<Vacancy>();
            await RetryPolicy.ExecuteAsync(context => collection.InsertOneAsync(vacancy), new Context(nameof(CreateAsync)));
        }

        public async Task<Vacancy> GetVacancyAsync(long vacancyReference)
        {
            var vacancy = await FindVacancy(v => v.VacancyReference, vacancyReference);

            if (vacancy == null)
                throw new VacancyNotFoundException(string.Format(ExceptionMessages.VacancyWithReferenceNotFound, vacancyReference));

            return vacancy;
        }

        public async Task<Vacancy> GetVacancyAsync(Guid id)
        {
            var vacancy = await FindVacancy(v => v.Id, id);

            if (vacancy == null)
                throw new VacancyNotFoundException(string.Format(ExceptionMessages.VacancyWithIdNotFound, id));

            return vacancy;
        }

        private async Task<Vacancy> FindVacancy<TField>(Expression<Func<Vacancy, TField>> expression, TField value)
        {
            var filter = Builders<Vacancy>.Filter.Eq(expression, value);
            var options = new FindOptions<Vacancy> { Limit = 1 };

            var collection = GetCollection<Vacancy>();
            var result = await RetryPolicy.ExecuteAsync(context => collection.FindAsync(filter, options), new Context(nameof(FindVacancy)));
            return result.SingleOrDefault();
        }

        public async Task<IEnumerable<T>> GetVacanciesByEmployerAccountAsync<T>(string employerAccountId)
        {
            var builder = Builders<T>.Filter;
            var filter = builder.Eq(EmployerAccountIdFieldName, employerAccountId) &
                        builder.Eq(OwnerTypeFieldName, OwnerType.Employer) &
                        builder.Ne(IsDeletedFieldName, true);

            var collection = GetCollection<T>();

            var options = new FindOptions<T> { Projection = GetProjection<T>() };

            var result = await RetryPolicy.ExecuteAsync(context => collection.FindAsync<T>(filter, options), 
                new Context(nameof(GetVacanciesByEmployerAccountAsync)));

            return await result.ToListAsync();
        }

        public async Task<Vacancy> GetSingleVacancyForPostcodeAsync(string postcode)
        {
            var builder = Builders<Vacancy>.Filter;
            var filter = builder.Eq(v => v.EmployerLocation.Postcode, postcode) &
                         builder.Ne(v => v.EmployerLocation.Latitude, null) &
                         builder.Ne(v => v.EmployerLocation.Longitude, null);

            var collection = GetCollection<Vacancy>();
            var result = await RetryPolicy.ExecuteAsync(context => collection.FindAsync(filter), new Context(nameof(GetSingleVacancyForPostcodeAsync)));
            
            return result
                        .ToList()
                        .OrderByDescending(x => x.VacancyReference)
                        .FirstOrDefault();
        }

        public async Task<IEnumerable<Vacancy>> GetVacanciesByStatusAsync(VacancyStatus status)
        {
            var filter = Builders<Vacancy>.Filter.Eq(v => v.Status, status);

            var collection = GetCollection<Vacancy>();
            var result = await RetryPolicy.ExecuteAsync(context => collection.FindAsync(filter), new Context(nameof(GetVacanciesByStatusAsync)));

            return await result.ToListAsync();
        }

        public async Task UpdateAsync(Vacancy vacancy)
        {
            var filter = Builders<Vacancy>.Filter.Eq(v => v.Id, vacancy.Id);
            var collection = GetCollection<Vacancy>();
            await RetryPolicy.ExecuteAsync(context => collection.ReplaceOneAsync(filter, vacancy), new Context(nameof(UpdateAsync)));
        }

        public async Task<IEnumerable<string>> GetDistinctEmployerAccountsAsync()
        {
            var filter = Builders<Vacancy>.Filter.Empty;
            var collection = GetCollection<Vacancy>();
            var ids = collection.Distinct(x => x.EmployerAccountId, filter);
            
            return await ids.ToListAsync();
        }
    }
}
