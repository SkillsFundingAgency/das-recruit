using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public interface IMinimumWage
    {
        DateTime ValidFrom { get; set; }
        decimal ApprenticeshipMinimumWage { get; set; }
        decimal NationalMinimumWageLowerBound { get; set; }
        decimal NationalMinimumWageUpperBound { get; set; }
    }
}