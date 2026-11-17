using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events.Interfaces;

public interface IVacancyEvent
{
    Guid VacancyId { get; }
}