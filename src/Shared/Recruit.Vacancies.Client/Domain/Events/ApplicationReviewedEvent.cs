using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    public class ApplicationReviewedEvent : EventBase, INotification, IVacancyEvent
    {
        public string EmployerAccountId { get; set; }
        public Guid VacancyId { get; set; }
        public long VacancyReference { get; set; }
        public Guid CandidateId { get; set; }
        public ApplicationReviewStatus Status { get; set; }
        public string CandidateFeedback { get; set; }
    }
}
