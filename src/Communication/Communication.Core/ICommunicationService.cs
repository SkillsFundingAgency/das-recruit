using System.Threading.Tasks;
using Communication.Types;

namespace Communication.Core
{
    public interface ICommunicationService
    {
        Task ProcessCommunicationRequestAsync(CommunicationRequest request);
        Task ProcessAggregateCommunicationRequestAsync(AggregateCommunicationRequest request);
        /// <summary>
        /// This will process the set of messages for an individual user
        /// </summary>
        Task ProcessAggregateCommunicationComposeRequestAsync(AggregateCommunicationComposeRequest request);
    }
}