using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public class StubNationalMinimumWageService : IGetMinimumWages
    {
        public decimal GetMinimumWage(DateTime date)
        {
            return 3.70m;
        }
    }

}
