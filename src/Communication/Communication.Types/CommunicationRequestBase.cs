using System;
using System.Collections.Generic;

namespace Communication.Types
{
    public abstract class CommunicationRequestBase : ICommunicationRequest
    {
        private readonly List<Entity> _entityIds;

        public Guid RequestId { get; }
        public DateTime RequestDateTime { get; }
        public string RecipientsResolver { get; }
        public string OriginatingService { get; }

        public IEnumerable<Entity> EntityIds => _entityIds;

        protected CommunicationRequestBase(string recipientsResolver, string originatingService)
        {
            RequestId = Guid.NewGuid();
            RequestDateTime = DateTime.UtcNow;

            RecipientsResolver = recipientsResolver;
            OriginatingService = originatingService;

            _entityIds = new List<Entity>();
        }

        protected void AddEntity<T>(string entityType, T entityId)
        {
            _entityIds.Add(new Entity(entityType, entityId));
        }
    }
}
