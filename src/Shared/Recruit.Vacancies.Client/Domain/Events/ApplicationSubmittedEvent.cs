using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    // Note: Doesn't implement IApplicationReviewEvent as it's an externally published event.
    public class ApplicationSubmittedEvent : EventBase, INotification
    {
        public Entities.Application Application { get; set; }
    }
}