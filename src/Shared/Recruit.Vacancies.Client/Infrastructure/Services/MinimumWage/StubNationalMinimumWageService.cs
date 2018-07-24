using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.MinimumWage
{
    public class StubNationalMinimumWageService : IGetMinimumWages
    {
        public decimal GetApprenticeNationalMinimumWage(DateTime date)
        {
            return 3.70m;
        }

        public WageRange GetNationalMinimumWageRange(DateTime date)
        {
            return new WageRange
            {
                MinimumWage = 4.20m,
                MaximumWage = 7.83m
            };
        }
    }

}
