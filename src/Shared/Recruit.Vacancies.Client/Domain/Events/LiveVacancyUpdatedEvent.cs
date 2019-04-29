using System;
using Esfa.Recruit.Vacancies.Client.Application;
using Esfa.Recruit.Vacancies.Client.Domain.Events.Interfaces;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
	public class LiveVacancyUpdatedEvent : EventBase, INotification, IVacancyEvent
    {
        public Guid VacancyId { get; private set; }
        public long VacancyReference { get; private set; }
        public LiveUpdateKind UpdateKind { get; private set; }

        public LiveVacancyUpdatedEvent(Guid vacancyId, long vacancyReference, LiveUpdateKind updateKind)
        {
            VacancyId = vacancyId;
            VacancyReference = vacancyReference;
            UpdateKind = updateKind;
        }
    }
}