using System;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    // Note: This is an externally published event.
    public class CandidateDeleteEvent : EventBase, INotification
    {
        public Guid CandidateId { get; set; }
    }
}
