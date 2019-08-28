using System;
using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Interfaces;

namespace Communication.Core
{
    public interface ICommunicationService
    {
        Task ProcessCommunicationRequestAsync(CommunicationRequest request);
        Task ProcessAggregateCommunicationRequestAsync(AggregateCommunicationRequest request);
        Task ProcessAggregateCommunicationComposeRequestAsync(AggregateCommunicationComposeRequest request);
    }
}