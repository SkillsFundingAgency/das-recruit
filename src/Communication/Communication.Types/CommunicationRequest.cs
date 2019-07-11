﻿using System;
using System.Collections.Generic;

namespace Communication.Types
{
    public class CommunicationRequest
    {
        public Guid RequestId { get; set ;}
        public string RequestType { get; set; }
        public DateTime RequestDateTime { get; set; }
        public string ParticipantsResolverName { get; set; }
        public string TemplateProviderName { get; set; }
        public List<Entity> Entities { get; set; }

        public CommunicationRequest()
        {
            RequestDateTime = DateTime.UtcNow;
            Entities = new List<Entity>();
        }

        public CommunicationRequest(string requestType, string participantsResolverName, string templateProviderName) : this()
        {
            RequestId = Guid.NewGuid();

            RequestType = requestType;
            ParticipantsResolverName = participantsResolverName;
            TemplateProviderName = templateProviderName;
        }

        public void AddEntity(string entityType, object entityId)
        {
            Entities.Add(new Entity(entityType, entityId));
        }
    }
}
