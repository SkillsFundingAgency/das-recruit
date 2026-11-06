using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events.Interfaces;

public interface IVacancyReviewEvent
{
    long VacancyReference { get; }
    Guid ReviewId { get; }
}