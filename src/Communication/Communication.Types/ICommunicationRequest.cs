using System;
using System.Collections.Generic;

namespace Communication.Types
{
    public interface ICommunicationRequest
    {
        Guid RequestId { get; }
        DateTime RequestDateTime { get; }
        string RecipientsResolver { get; }
        string OriginatingService { get; }
        IEnumerable<Entity> EntityIds { get; }
    }
}
