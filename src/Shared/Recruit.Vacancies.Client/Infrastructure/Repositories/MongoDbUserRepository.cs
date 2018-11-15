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
            var result = await RetryPolicy.ExecuteAsync(context => collection.FindAsync(filter), new Context(nameof(GetAsync)));
            return result.SingleOrDefault();
        }
        
        public Task UpsertUserAsync(User user)
        {
            var filter = Builders<User>.Filter.Eq(v => v.Id, user.Id);
            var collection = GetCollection<User>();
            return RetryPolicy.ExecuteAsync(context => collection.ReplaceOneAsync(filter, user, new UpdateOptions { IsUpsert = true }), new Context(nameof(UpsertUserAsync)));
        }
    }
}
