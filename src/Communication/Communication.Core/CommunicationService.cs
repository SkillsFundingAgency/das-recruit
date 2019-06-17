using System.Threading.Tasks;
using Communication.Types;
using Microsoft.Extensions.Logging;

namespace Communication.Core
{
    public class CommunicationService : ICommunicationService
    {
        private readonly ILogger<CommunicationService> _logger;
        private readonly ICommunicationProcessor _processor;
        private readonly ICommunicationRepository _repository;

        public CommunicationService(ILogger<CommunicationService> logger, ICommunicationProcessor processor, ICommunicationRepository repository)
        {
            _logger = logger;
            _processor = processor;
            _repository = repository;
        }

        public async Task ProcessRequest(CommunicationRequest req)
        {
            var messages = await _processor.CreateMessagesAsync(req);
        }
    }
}