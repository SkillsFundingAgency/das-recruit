using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    public interface IVacancyReferenceEvent
    {
        long VacancyReference { get; }
        Guid VacancyReviewId { get; }
    }
}