using System;
using System.Collections.Generic;

namespace Communication.Types
{
    public class AggregateCommunicationComposeRequest
    {
        public string UserId { get; }
        public IEnumerable<Guid> MessageIds { get; }
        public AggregateCommunicationRequest AggregateCommunicationRequest { get; }

        public AggregateCommunicationComposeRequest(string userId, IEnumerable<Guid> messageIds, AggregateCommunicationRequest aggregateCommunicationRequest)
        {
            UserId = userId;
            MessageIds = messageIds;
            AggregateCommunicationRequest = aggregateCommunicationRequest;
        }
    }
}