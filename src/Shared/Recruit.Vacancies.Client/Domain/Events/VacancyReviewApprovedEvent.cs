using System;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    public class VacancyReviewApprovedEvent : EventBase, INotification, IVacancyReferenceEvent
    {
        public long VacancyReference { get; set; }

        public Guid ReviewId { get; set; }
    }
}