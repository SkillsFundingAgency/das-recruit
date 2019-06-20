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
        public string OriginatingService { get; }
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
