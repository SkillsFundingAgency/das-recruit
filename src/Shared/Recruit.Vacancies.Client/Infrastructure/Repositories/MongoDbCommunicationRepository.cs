using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    public sealed class MongoDbCommunicationRepository : MongoDbCollectionBase, ICommunicationRepository
    {
        private const string CommunicationMessagesCollectionName = "communicationMessages";
        public MongoDbCommunicationRepository(ILoggerFactory loggerFactory, IOptions<MongoDbConnectionDetails> details)
            : base(loggerFactory, MongoDbNames.RecruitDb, CommunicationMessagesCollectionName, details)
        {
        }

        public Task InsertAsync(CommunicationMessage msg)
        {
            var collection = GetCollection<CommunicationMessage>();
            return RetryPolicy.ExecuteAsync(_ =>
                collection.InsertOneAsync(msg),
                new Context(nameof(InsertAsync)));
        }

        public async Task<CommunicationMessage> GetAsync(Guid msgId)
        {
            var filter = Builders<CommunicationMessage>.Filter.Eq(cm => cm.Id, msgId);
            var collection = GetCollection<CommunicationMessage>();
            var result = await RetryPolicy.ExecuteAsync(_ =>
                collection.Find(filter)
                .SingleOrDefaultAsync(),
                new Context(nameof(GetAsync)));
            return result;
        }

        public Task UpdateAsync(CommunicationMessage commMsg)
        {
            var filter = Builders<CommunicationMessage>.Filter.Eq(cm => cm.Id, commMsg.Id);
            var collection = GetCollection<CommunicationMessage>();
            return RetryPolicy.ExecuteAsync(_ =>
                collection.ReplaceOneAsync(filter, commMsg),
                new Context(nameof(UpdateAsync)));
        }

        public Task HardDelete(DateTime dispatchDateTime)
        {
            var builder = Builders<CommunicationMessage>.Filter;
            var filter = (builder.Eq(cm => cm.Status, CommunicationMessageStatus.Sent) |
                        builder.Eq(cm => cm.Status, CommunicationMessageStatus.NotSent)) &
                        builder.Lte(cm => cm.DispatchDateTime, dispatchDateTime);
            var collection = GetCollection<CommunicationMessage>();

            return RetryPolicy.ExecuteAsync(_ =>
                collection.DeleteManyAsync(filter),
                new Context(nameof(HardDelete)));
        }
    }
}
