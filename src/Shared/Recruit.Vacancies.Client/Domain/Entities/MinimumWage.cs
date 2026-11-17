using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

public class MinimumWage
{
    public DateTime ValidFrom { get; set; }
    public decimal ApprenticeshipMinimumWage { get; set; }
    public decimal NationalMinimumWageLowerBound { get; set; }
    public decimal NationalMinimumWageUpperBound { get; set; }
}