using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
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
            var filter = Builders<User>.Filter.Regex(v => v.IdamsUserId, 
                new BsonRegularExpression(Regex.Escape(idamsUserId.ToLower()),"i" ));

            var collection = GetCollection<User>();
            var result = await RetryPolicy.ExecuteAsync(_ => 
                collection.Find(filter, new FindOptions{})
                .FirstOrDefaultAsync(),
                new Context(nameof(GetAsync)));
            return result;
        }
        
        public async Task<User> GetByDfEUserId(string dfEUserId)
        {
            var filter = Builders<User>.Filter.Regex(v => v.DfEUserId, 
                new BsonRegularExpression(Regex.Escape(dfEUserId.ToLower()),"i" ));

            var collection = GetCollection<User>();
            var result = await RetryPolicy.ExecuteAsync(_ => 
                    collection.Find(filter)
                        .FirstOrDefaultAsync(),
                new Context(nameof(GetAsync)));
            return result;
        }

        public Task UpsertUserAsync(User user)
        {
            var filter = Builders<User>.Filter.Eq(v => v.Id, user.Id);
            var collection = GetCollection<User>();
            return RetryPolicy.ExecuteAsync(_ => 
                collection.ReplaceOneAsync(filter, user, new ReplaceOptions { IsUpsert = true }),
                new Context(nameof(UpsertUserAsync)));
        }

        public Task<List<User>> GetEmployerUsersAsync(string accountId)
        {
            var filter = Builders<User>.Filter.AnyEq(u => u.EmployerAccountIds, accountId);
            var collection = GetCollection<User>();
            return RetryPolicy.ExecuteAsync(_ => 
                collection.Find(filter).ToListAsync(),
                new Context(nameof(GetEmployerUsersAsync)));
        }

        public Task<List<User>> GetProviderUsersAsync(long ukprn)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Ukprn, ukprn);
            var collection = GetCollection<User>();
            return RetryPolicy.ExecuteAsync(_ => 
                collection.Find(filter).ToListAsync(),
                new Context(nameof(GetProviderUsersAsync)));
        }
        
        public async Task<User> GetUserByEmail(string email, UserType userType)
        {
            var builder = Builders<User>.Filter;
            var filter = builder.Regex(v => v.Email, new BsonRegularExpression(Regex.Escape(email.ToLower()),"i" )) &
                         builder.Eq(cm => cm.UserType, userType);
            
            var collection = GetCollection<User>();
            var result = await RetryPolicy.ExecuteAsync(_ => 
                    collection.Find(filter)
                        .FirstOrDefaultAsync(),
                new Context(nameof(MongoDbUserRepository.GetAsync)));
            return result;
        }
    }
}
