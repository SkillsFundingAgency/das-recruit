using System;
using Esfa.Recruit.Vacancies.Client.Domain.Events.Interfaces;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    public class VacancyReviewCreatedEvent : EventBase, INotification, IVacancyReviewEvent
    {
        public long VacancyReference { get; set; }

        public Guid ReviewId { get; set; }
    }
}