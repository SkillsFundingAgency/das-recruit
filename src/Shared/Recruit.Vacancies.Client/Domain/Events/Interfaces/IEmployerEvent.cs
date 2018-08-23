using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events.Interfaces
{
    public interface IEmployerEvent
    {
        string EmployerAccountId { get; }
    }
}