using System.Threading.Tasks;
using System.Linq;
using Communication.Types;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Communication.Core
{
    public class CommunicationService : ICommunicationService
    {
        private readonly ILogger<CommunicationService> _logger;
        private readonly ICommunicationProcessor _processor;
        private readonly ICommunicationRepository _repository;
        private readonly IDispatchQueuePublisher _publisher;

        public CommunicationService(ILogger<CommunicationService> logger, ICommunicationProcessor processor,
                                    ICommunicationRepository repository, IDispatchQueuePublisher publisher)
        {
            _logger = logger;
            _processor = processor;
            _repository = repository;
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

        private Task QueueImmediateDispatchManyAsync(IEnumerable<CommunicationMessage> messages)
        {
            var msgIds = messages.Select(m => new CommunicationMessageIdentifier(m.Id));
            return Task.WhenAll(msgIds.Select(m => _publisher.AddMessageAsync(m)));
        }
    }
}