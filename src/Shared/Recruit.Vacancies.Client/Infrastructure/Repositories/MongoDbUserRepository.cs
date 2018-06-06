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
        private const string Database = "recruit";
        private const string Collection = "users";

        public MongoDbUserRepository(ILogger<MongoDbUserRepository> logger,  IOptions<MongoDbConnectionDetails> details)
            : base(logger, Database, Collection, details)
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
