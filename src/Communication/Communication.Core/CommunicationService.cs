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

        public async Task ProcessRequest(CommunicationRequest req)
        {
            var messages = (await _processor.CreateMessagesAsync(req)).ToList();

            await Task.WhenAll(messages.Select(m => _repository.InsertAsync(m)));
            var msgIds = messages.Select(m => new CommunicationMessageIdentifier { Id = m.Id });
            await Task.WhenAll(msgIds.Select(m => _publisher.AddMessageAsync(m)));
        }
    }
}