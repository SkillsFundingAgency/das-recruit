using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    public interface IVacancyReferenceEvent
    {
        long VacancyReference { get; }
        Guid ReviewId { get; }
    }
}