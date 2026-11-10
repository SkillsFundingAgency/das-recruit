using System;
using Esfa.Recruit.Vacancies.Client.Domain.Events.Interfaces;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events;

public class VacancyTransferredEvent : EventBase, INotification, IVacancyEvent
{
    public Guid VacancyId { get; set; }
    public long VacancyReference { get; set; }
}