using System;
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
    internal sealed class MongoDbUserNotificationPreferencesRepository : MongoDbCollectionBase, IUserNotificationPreferencesRepository
    {
        public MongoDbUserNotificationPreferencesRepository(ILoggerFactory loggerFactory, IOptions<MongoDbConnectionDetails> config) 
            : base(loggerFactory, MongoDbNames.RecruitDb, MongoDbCollectionNames.UserNotificationPreferences, config)
        {
        }

        public async Task<UserNotificationPreferences> GetAsync(string idamsUserId)
        {   
            var filter = Builders<UserNotificationPreferences>.Filter.Regex(v => v.Id, 
                new BsonRegularExpression(Regex.Escape(idamsUserId.ToLower()),"i" ));

            var collection = GetCollection<UserNotificationPreferences>();
            var result = await RetryPolicy.ExecuteAsync(_ => 
                collection.Find(filter)
                .FirstOrDefaultAsync(),
                new Context(nameof(GetAsync)));
            return result;
        }

        public async Task<UserNotificationPreferences> GetByDfeUserId(string dfeUserId)
        {
            if (string.IsNullOrEmpty(dfeUserId))
            {
                return null;
            }
            var filter = Builders<UserNotificationPreferences>.Filter.Regex(v => v.DfeUserId, 
                new BsonRegularExpression(Regex.Escape(dfeUserId.ToLower()),"i" ));

            var collection = GetCollection<UserNotificationPreferences>();
            var result = await RetryPolicy.ExecuteAsync(_ => 
                    collection.Find(filter)
                        .SingleOrDefaultAsync(),
                new Context(nameof(GetAsync)));
            return result;
        }

        public async Task UpsertAsync(UserNotificationPreferences preferences)
        {
            var isDfeSignInUserWithPreferencesSaved = false;

            if (!string.IsNullOrEmpty(preferences.DfeUserId))
            {
                var dfepreferences = await GetByDfeUserId(preferences.DfeUserId);
                if (dfepreferences != null)
                {
                    isDfeSignInUserWithPreferencesSaved = true;
                }
            }
            
            var filter = !isDfeSignInUserWithPreferencesSaved
                ? Builders<UserNotificationPreferences>.Filter.Regex(v => v.Id, new BsonRegularExpression(Regex.Escape(preferences.Id.ToLower()),"i" ))
                : Builders<UserNotificationPreferences>.Filter.Eq(v => v.DfeUserId, preferences.DfeUserId);
            
            var collection = GetCollection<UserNotificationPreferences>();
            
            await RetryPolicy.ExecuteAsync(_ => 
                collection.ReplaceOneAsync(filter, preferences, new ReplaceOptions { IsUpsert = true }),
                new Context(nameof(UpsertAsync)));
        }
    }
}