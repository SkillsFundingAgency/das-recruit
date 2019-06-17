using System;

namespace Communication.Types
{
    public abstract class AggregateCommunicationRequestBase : IAggregateCommunicationRequest
    {
        public Guid RequestId { get; }
        public DateTime RequestDateTime { get; }

        protected AggregateCommunicationRequestBase()
        {
            RequestId = Guid.NewGuid();
            RequestDateTime = DateTime.UtcNow;
        }
    }
}
