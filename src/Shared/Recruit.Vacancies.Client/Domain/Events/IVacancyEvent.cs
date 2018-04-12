using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    public interface IVacancyEvent
    {
        string EmployerAccountId { get; }
        Guid VacancyId { get; }
    }
}