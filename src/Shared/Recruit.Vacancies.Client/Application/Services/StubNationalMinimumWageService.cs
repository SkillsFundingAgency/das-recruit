using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public class StubNationalMinimumWageService : IGetApprenticeshipNationalMinimumWages
    {
        public decimal GetMinimumWage(DateTime date)
        {
            return 3.70m;
        }
    }

}
