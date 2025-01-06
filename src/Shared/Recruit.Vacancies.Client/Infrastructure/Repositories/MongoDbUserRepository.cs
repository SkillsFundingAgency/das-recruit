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
    internal sealed class MongoDbUserRepository : MongoDbCollectionBase, IUserRepository
    {
        public MongoDbUserRepository(ILoggerFactory loggerFactory, IOptions<MongoDbConnectionDetails> details)
            : base(loggerFactory, MongoDbNames.RecruitDb, MongoDbCollectionNames.Users, details)
        {
        }

        public async Task<User> GetAsync(string idamsUserId)
        {
            var filter = Builders<User>.Filter.Eq(v => v.IdamsUserId, idamsUserId);

            var collection = GetCollection<User>();
            var result = await RetryPolicy.Execute(_ => 
                collection.Find(filter)
                .FirstOrDefaultAsync(),
                new Context(nameof(GetAsync)));
            return result;
        }
        
        public async Task<User> GetByDfEUserId(string dfEUserId)
        {
            var filter = Builders<User>.Filter.Eq(v => v.DfEUserId, dfEUserId);

            var collection = GetCollection<User>();
            var result = await RetryPolicy.Execute(_ => 
                    collection.Find(filter)
                        .SingleOrDefaultAsync(),
                new Context(nameof(GetAsync)));
            return result;
        }
        
        public Task UpsertUserAsync(User user)
        {
            var filter = Builders<User>.Filter.Eq(v => v.Id, user.Id);
            var collection = GetCollection<User>();
            return RetryPolicy.Execute(_ => 
                collection.ReplaceOneAsync(filter, user, new ReplaceOptions { IsUpsert = true }),
                new Context(nameof(UpsertUserAsync)));
        }

        public Task<List<User>> GetEmployerUsersAsync(string accountId)
        {
            var filter = Builders<User>.Filter.AnyEq(u => u.EmployerAccountIds, accountId);
            var collection = GetCollection<User>();
            return RetryPolicy.Execute(_ => 
                collection.Find(filter).ToListAsync(),
                new Context(nameof(GetEmployerUsersAsync)));
        }

        public Task<List<User>> GetProviderUsersAsync(long ukprn)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Ukprn, ukprn);
            var collection = GetCollection<User>();
            return RetryPolicy.Execute(_ => 
                collection.Find(filter).ToListAsync(),
                new Context(nameof(GetProviderUsersAsync)));
        }
    }
}
