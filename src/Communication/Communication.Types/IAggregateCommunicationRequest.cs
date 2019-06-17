using System;

namespace Communication.Types
{
    public interface IAggregateCommunicationRequest
    {
        Guid RequestId { get; }
        DateTime RequestDateTime { get; }
    }
}
