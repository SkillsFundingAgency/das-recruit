using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public class StubNationalMinimumWageService : IGetMinimumWages
    {
        public decimal GetApprenticeNationalMinimumWage(DateTime date)
        {
            return 3.70m;
        }

        public Tuple<decimal, decimal> GetNationalMinimumWageRange(DateTime date)
        {
            return new Tuple<decimal, decimal>(4.05m, 7.83m);
        }
    }

}
