using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    internal sealed class MongoDbEmployerProfileRepository : MongoDbCollectionBase, IEmployerProfileRepository
    {
        public MongoDbEmployerProfileRepository(ILoggerFactory loggerFactory, IOptions<MongoDbConnectionDetails> details) 
            : base(loggerFactory, MongoDbNames.RecruitDb, MongoDbCollectionNames.EmployerProfiles, details)
        {
        }

        public async Task CreateAsync(EmployerProfile profile)
        {
            var collection = GetCollection<EmployerProfile>();
            await RetryPolicy.Execute(_ => 
                collection.InsertOneAsync(profile),
                new Context(nameof(CreateAsync)));
        }

        public async Task<EmployerProfile> GetAsync(string employerAccountId, string accountLegalEntityPublicHashedId)
        {
            var builder = Builders<EmployerProfile>.Filter;
            var filter = builder.Eq(x => x.Id, EmployerProfile.GetId(employerAccountId, accountLegalEntityPublicHashedId));

            var collection = GetCollection<EmployerProfile>();

            var result = await RetryPolicy.Execute(_ => 
                collection.Find(filter).SingleOrDefaultAsync(),
                new Context(nameof(GetAsync)));
            
            return result;
        }

        public async Task<IList<EmployerProfile>> GetEmployerProfilesForEmployerAsync(string employerAccountId)
        {
            var filter = Builders<EmployerProfile>.Filter.Eq(x => x.EmployerAccountId, employerAccountId);

            var collection = GetCollection<EmployerProfile>();

            var result = await RetryPolicy.Execute(_ => 
                collection.Find(filter).ToListAsync(),
                new Context(nameof(GetEmployerProfilesForEmployerAsync)));
    
            return result;
        }

        public Task UpdateAsync(EmployerProfile profile)
        {
            var builder = Builders<EmployerProfile>.Filter;
            var filter = builder.Eq(x => x.EmployerAccountId, profile.EmployerAccountId) &
                         builder.Eq(x => x.AccountLegalEntityPublicHashedId, profile.AccountLegalEntityPublicHashedId);

            var collection = GetCollection<EmployerProfile>();

            return RetryPolicy.Execute(_ => 
                collection.ReplaceOneAsync(filter, profile),
                new Context(nameof(UpdateAsync)));
        }
    }
}
