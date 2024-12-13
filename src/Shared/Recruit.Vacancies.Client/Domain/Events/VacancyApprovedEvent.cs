using System;
using Esfa.Recruit.Vacancies.Client.Domain.Events.Interfaces;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    public class VacancyApprovedEvent : EventBase, INotification, IVacancyEvent, NServiceBus.IEvent
    {
        public string AccountLegalEntityPublicHashedId { get; set; }
        public long Ukprn { get; set; }
        public Guid VacancyId { get; set; }
        public long VacancyReference { get; set; }
    }
}
