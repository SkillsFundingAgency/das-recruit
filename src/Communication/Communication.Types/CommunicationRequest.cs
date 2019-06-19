using System;
using System.Collections.Generic;

namespace Communication.Types
{
    public class CommunicationRequest
    {
        public Guid RequestId { get; }
        public string RequestType { get; }
        public DateTime RequestDateTime { get; }
        public string RecipientsResolver { get; }
        public string OriginatingService { get; }
        public List<Entity> Entities { get; }
        public CommunicationRequest(string requestType, string recipientsResolver, string originatingService)
        {
            RequestId = Guid.NewGuid();
            RequestDateTime = DateTime.UtcNow;

            RequestType = requestType;
            RecipientsResolver = recipientsResolver;
            OriginatingService = originatingService;

            Entities = new List<Entity>();
        }

        public void AddEntity<T>(string entityType, T entityId)
        {
            Entities.Add(new Entity(entityType, entityId));
        }
    }
}
