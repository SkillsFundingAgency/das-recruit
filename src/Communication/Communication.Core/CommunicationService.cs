using System.Threading.Tasks;
using System.Linq;
using Communication.Types;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;

namespace Communication.Core
{
    public class CommunicationService : ICommunicationService
    {
        private readonly DeliveryFrequency[] _digestDeliveryFrequencies = new [] { DeliveryFrequency.Daily, DeliveryFrequency.Weekly };
        private readonly ILogger<CommunicationService> _logger;
        private readonly ICommunicationProcessor _processor;
        private readonly IAggregateCommunicationProcessor _aggregateProcessor;
        private readonly ICommunicationRepository _repository;
        private readonly IAggregateCommunicationComposeQueuePublisher _composerQueue;
        private readonly IDispatchQueuePublisher _publisher;

        public CommunicationService(ILogger<CommunicationService> logger, ICommunicationProcessor processor, IAggregateCommunicationProcessor aggregateCommunicationProcessor,
                                    ICommunicationRepository repository, IAggregateCommunicationComposeQueuePublisher composerQueue, IDispatchQueuePublisher publisher)
        {
            _logger = logger;
            _processor = processor;
            _aggregateProcessor = aggregateCommunicationProcessor;
            _repository = repository;
            _composerQueue = composerQueue;
            _publisher = publisher;
        }

        public async Task ProcessCommunicationRequestAsync(CommunicationRequest request)
        {
            var messages = await _processor.CreateMessagesAsync(request);

            _logger.LogInformation($"Generated {messages.Count()} messages for Communication Request: {request.RequestId.ToString()} - {request.RequestType}");

            await Task.WhenAll(messages.Select(m => _repository.InsertAsync(m)));

            if (messages.Any(m => m.Frequency == DeliveryFrequency.Immediate))
            {
                var immediateMessages = messages.Where(m => m.Frequency == DeliveryFrequency.Immediate).ToList();
                _logger.LogInformation($"Queueing {immediateMessages.Count()} communication messages to dispatch. For Communication Request: {request.RequestId.ToString()} - {request.RequestType}");
                await QueueImmediateDispatchManyAsync(immediateMessages);
            }
        }

        public async Task ProcessAggregateCommunicationRequestAsync(AggregateCommunicationRequest request)
        {
            if (_digestDeliveryFrequencies.Contains(request.Frequency) == false)
                throw new NotSupportedException(CommunicationErrorMessages.InvalidAggregateCommunicationRequestedFrequencyMessage);

            var scheduledMessages = await _repository.GetScheduledMessagesSinceAsync(request.RequestType, request.Frequency, request.FromDateTime, request.ToDateTime);

            var groupedUserMessages = scheduledMessages.GroupBy(cm => cm.Recipient.UserId);

            _logger.LogInformation($"Found {groupedUserMessages.Count()} users to possibly send {request.RequestType} aggregate emails to. For Communication Request: {request.RequestId.ToString()}");

            var tasks = new List<Task>();

            foreach (var userMessages in groupedUserMessages)
            {
                var acr = new AggregateCommunicationRequest(Guid.NewGuid(), request.RequestType, request.Frequency, request.RequestDateTime, request.FromDateTime, request.ToDateTime);
                var tsk = _composerQueue.AddMessageAsync(new AggregateCommunicationComposeRequest(
                    userId: userMessages.Key,
                    messageIds: userMessages.Select(cm => cm.Id),
                    aggregateCommunicationRequest: acr
                ));

                tasks.Add(tsk);
            }

            await Task.WhenAll(tasks);
        }

        public async Task ProcessAggregateCommunicationComposeRequestAsync(AggregateCommunicationComposeRequest request)
        {
            var originallyScheduledCommunicationMessages = await _repository.GetManyAsync(request.MessageIds);
            var aggregatedMessage = await _aggregateProcessor.CreateAggregateMessageAsync(request.AggregateCommunicationRequest, originallyScheduledCommunicationMessages);

            if (aggregatedMessage != null)
            {
                await _repository.InsertAsync(aggregatedMessage);
                await MarkOriginallyScheduledMessagesAsSentAsync(request.MessageIds, aggregatedMessage.Id);
                await QueueImmediateDispatchAsync(aggregatedMessage);
            }
            else
            {
                var msg = $@"After aggregating {request.MessageIds.Count()} {request.AggregateCommunicationRequest.Frequency.ToString()} -
                {request.AggregateCommunicationRequest.RequestType} notifications for {request.UserId}, there was nothing to send.";
                _logger.LogInformation(msg);
            }
        }

        private Task MarkOriginallyScheduledMessagesAsSentAsync(IEnumerable<Guid> oldMsgIds, Guid aggregatedMessageId)
        {
            return _repository.UpdateScheduledMessagesAsSentAsync(oldMsgIds, aggregatedMessageId);
        }

        private Task QueueImmediateDispatchAsync(CommunicationMessage message)
        {
            return _publisher.AddMessageAsync( new CommunicationMessageIdentifier(message.Id));
        }

        private Task QueueImmediateDispatchManyAsync(IEnumerable<CommunicationMessage> messages)
        {
            var msgIds = messages.Select(m => new CommunicationMessageIdentifier(m.Id));
            return Task.WhenAll(msgIds.Select(m => _publisher.AddMessageAsync(m)));
        }
    }
}