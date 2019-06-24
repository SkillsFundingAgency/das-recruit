using System;
using System.Collections.Generic;

namespace Communication.Types
{
    public class CommunicationRequest
    {
        public Guid RequestId { get; }
        public string RequestType { get; }
        public DateTime RequestDateTime { get; }
        public string ParticipantsResolverName { get; }
        /// This will be used to discover various plugins required to process the message
        public string OriginatingServiceName { get; }
        public List<Entity> Entities { get; }
        public CommunicationRequest(string requestType, string participantsResolverName, string originatingService)
        {
            RequestId = Guid.NewGuid();
            RequestDateTime = DateTime.UtcNow;

            RequestType = requestType;
            ParticipantsResolverName = participantsResolverName;
            OriginatingService = originatingService;

            Entities = new List<Entity>();
        }

        public void AddEntity(string entityType, object entityId)
        {
            Entities.Add(new Entity(entityType, entityId));
        }
    }
}
