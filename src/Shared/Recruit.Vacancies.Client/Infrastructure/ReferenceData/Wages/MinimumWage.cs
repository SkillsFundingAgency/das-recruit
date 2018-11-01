using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Wages
{
    public class MinimumWage : IMinimumWage
    {
        public DateTime ValidFrom { get; set; }
        public decimal ApprenticeshipMinimumWage { get; set; }
        public decimal NationalMinimumWageLowerBound { get; set; }
        public decimal NationalMinimumWageUpperBound { get; set; }
    }
}