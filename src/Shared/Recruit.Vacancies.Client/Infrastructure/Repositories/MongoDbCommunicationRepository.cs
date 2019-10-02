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

        public async Task<IEnumerable<CommunicationMessage>> GetManyAsync(IEnumerable<Guid> msgIds)
        {
            var filter = Builders<CommunicationMessage>.Filter.In(cm => cm.Id, msgIds);
            var collection = GetCollection<CommunicationMessage>();
            var result = await RetryPolicy.ExecuteAsync(_ =>
                collection.FindAsync(filter),
                new Context(nameof(GetManyAsync)));
            return await result.ToListAsync();
        }

        public Task UpdateAsync(CommunicationMessage commMsg)
        {
            var filter = Builders<CommunicationMessage>.Filter.Eq(cm => cm.Id, commMsg.Id);
            var collection = GetCollection<CommunicationMessage>();
            return RetryPolicy.ExecuteAsync(_ =>
                collection.ReplaceOneAsync(filter, commMsg),
                new Context(nameof(UpdateAsync)));
        }

        public async Task<IEnumerable<CommunicationMessage>> GetScheduledMessagesSinceAsync(string requestType, DeliveryFrequency frequency, DateTime from, DateTime to)
        {
            var builder = Builders<CommunicationMessage>.Filter;
            var filter = builder.Eq(cm => cm.RequestType, requestType) &
                        builder.Eq(cm => cm.Frequency, frequency) &
                        builder.Eq(cm => cm.Status, CommunicationMessageStatus.Pending) &
                        builder.Gte(cm => cm.RequestDateTime, from) &
                        builder.Lte(cm => cm.RequestDateTime, to);
            var collection = GetCollection<CommunicationMessage>();
            var result = await RetryPolicy.ExecuteAsync(_ =>
                collection.FindAsync(filter),
                new Context(nameof(GetScheduledMessagesSinceAsync)));
            return await result.ToListAsync();
        }

        public Task UpdateScheduledMessagesAsSentAsync(IEnumerable<Guid> msgIds, Guid aggregatedMessageId)
        {
            var builder = Builders<CommunicationMessage>.Filter;
            var filter = builder.Ne(cm => cm.Frequency, DeliveryFrequency.Immediate) &
                        builder.In(cm => cm.Id, msgIds);
            var collection = GetCollection<CommunicationMessage>();

            var updateDef = new UpdateDefinitionBuilder<CommunicationMessage>()
                                .Set(cm => cm.Status, CommunicationMessageStatus.Sent)
                                .Set(cm => cm.AggregatedMessageId, aggregatedMessageId);

            return RetryPolicy.ExecuteAsync(_ =>
                collection.UpdateManyAsync(filter, updateDef),
                new Context(nameof(UpdateScheduledMessagesAsSentAsync)));
        }
    }
}
