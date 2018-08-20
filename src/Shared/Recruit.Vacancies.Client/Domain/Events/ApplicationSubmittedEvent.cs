using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    public class ApplicationSubmittedEvent : EventBase, INotification
    {
        public Entities.Application Application { get; set; }
    }
}