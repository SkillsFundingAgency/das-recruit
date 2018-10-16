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
        public MongoDbEmployerProfileRepository(ILogger<MongoDbEmployerProfileRepository> logger, IOptions<MongoDbConnectionDetails> details) 
            : base(logger, MongoDbNames.RecruitDb, MongoDbCollectionNames.EmployerProfiles, details)
        {
        }

        public async Task CreateAsync(EmployerProfile profile)
        {
            var collection = GetCollection<EmployerProfile>();
            await RetryPolicy.ExecuteAsync(context => collection.InsertOneAsync(profile), new Context(nameof(CreateAsync)));
        }

        public async Task<IList<EmployerProfile>> GetEmployerProfilesForEmployerAsync(string employerAccountId)
        {
            // TODO: LWA - Refactor when Chris's PR is merged
            var filter = Builders<EmployerProfile>.Filter.Eq(x => x.EmployerAccountId, employerAccountId);

            var collection = GetCollection<EmployerProfile>();

            var result = await RetryPolicy.ExecuteAsync(context => collection.FindAsync(filter), new Context(nameof(GetEmployerProfilesForEmployerAsync)));
    
            return await result.ToListAsync();
        }
    }
}
