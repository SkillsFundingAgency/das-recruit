using System;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    public class LiveVacancyClosingDateChangedEvent : EventBase, INotification
    {
        public Guid VacancyId { get; }
        public long VacancyReference { get; }
        public DateTime NewClosingDate { get; }

        public LiveVacancyClosingDateChangedEvent(Guid vacancyId, long vacancyReference, DateTime newClosingDate)
        {
            VacancyId = vacancyId;
            VacancyReference = vacancyReference;
            NewClosingDate = newClosingDate;
        }
    }
}
