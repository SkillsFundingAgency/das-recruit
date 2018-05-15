using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    public interface IVacancyReviewEvent
    {
        long VacancyReference { get; }
        Guid ReviewId { get; }
    }
}