using System.Threading.Tasks;
using Communication.Types;

namespace Communication.Core
{
    public interface ICommunicationService
    {
        Task ProcessCommunicationRequestAsync(CommunicationRequest request);
        Task ProcessAggregateCommunicationRequestAsync(AggregateCommunicationRequest request);
        Task ProcessAggregateCommunicationComposeRequestAsync(AggregateCommunicationComposeRequest request);
    }
}