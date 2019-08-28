using System;
using System.Collections.Generic;

namespace Communication.Types
{
    public class AggregateCommunicationComposeRequest
    {
        public string UserId { get; set; }
        public IEnumerable<Guid> MessageIds { get; set; }
        public AggregateCommunicationRequest AggregateCommunicationRequest { get; set; }
    }
}