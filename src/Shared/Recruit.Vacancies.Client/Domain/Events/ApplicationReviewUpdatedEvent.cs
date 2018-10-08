using System;
using Esfa.Recruit.Vacancies.Client.Domain.Events.Interfaces;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    public class ApplicationReviewUpdatedEvent : EventBase, INotification, IApplicationReviewEvent
    {
        public string EmployerAccountId { get; set; }
        public long VacancyReference { get; set; }
    }
}
