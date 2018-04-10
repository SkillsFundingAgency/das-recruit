using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    public interface IVacancyEvent
    {
         Guid VacancyId { get; }
    }
}