using System.Threading.Tasks;
using Communication.Types;

namespace Communication.Core
{
    public class CommunicationService : ICommunicationService
    {
        private readonly ICommunicationProcessor _processor;
        private readonly ICommunicationRepository _repository;

        public CommunicationService(ICommunicationProcessor processor, ICommunicationRepository repository)
        {
            _processor = processor;
            _repository = repository;
        }

        public Task ProcessRequest(CommunicationRequest req)
        {
            throw new System.NotImplementedException();
        }
    }
}