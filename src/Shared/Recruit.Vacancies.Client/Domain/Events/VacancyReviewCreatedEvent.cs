
using System;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    public class VacancyReviewCreatedEvent : EventBase, INotification, IVacancyReferenceEvent
    {
        public long VacancyReference { get; set; }

        public Guid VacancyReviewId { get; set; }
    }
}