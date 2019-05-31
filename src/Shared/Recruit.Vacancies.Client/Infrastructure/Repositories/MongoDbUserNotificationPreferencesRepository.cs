using System;
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
    internal sealed class MongoDbUserNotificationPreferencesRepository : MongoDbCollectionBase, IUserNotificationPreferencesRepository
    {
        public MongoDbUserNotificationPreferencesRepository(ILoggerFactory loggerFactory, IOptions<MongoDbConnectionDetails> config) 
            : base(loggerFactory, MongoDbNames.RecruitDb, MongoDbCollectionNames.UserNotificationPreferences, config)
        {
        }

        public async Task<UserNotificationPreferences> GetAsync(Guid userId)
        {
            var filter = Builders<UserNotificationPreferences>.Filter.Eq(v => v.Id, userId);

            var collection = GetCollection<UserNotificationPreferences>();
            var result = await RetryPolicy.ExecuteAsync(_ => 
                collection.Find(filter)
                .SingleOrDefaultAsync(),
                new Context(nameof(GetAsync)));
            return result;
        }

        public Task UpsertAsync(UserNotificationPreferences preferences)
        {
            var filter = Builders<UserNotificationPreferences>.Filter.Eq(v => v.Id, preferences.Id);
            var collection = GetCollection<UserNotificationPreferences>();
            return RetryPolicy.ExecuteAsync(_ => 
                collection.ReplaceOneAsync(filter, preferences, new UpdateOptions { IsUpsert = true }),
                new Context(nameof(UpsertAsync)));
        }
    }
}