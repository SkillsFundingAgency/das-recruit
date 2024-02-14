using System;
using Esfa.Recruit.Vacancies.Client.Domain.Events.Interfaces;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    public class VacancyDeletedEvent : EventBase, INotification, IVacancyEvent, NServiceBus.IEvent
    {
        public Guid VacancyId { get; set; }
    }
}