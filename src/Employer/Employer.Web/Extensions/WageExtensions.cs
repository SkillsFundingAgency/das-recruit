using System;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    using Esfa.Recruit.Vacancies.Client.Domain.Entities;
    using System.Linq;

    public static class WageExtensions
    {
        public static string ToText(this Wage wage, Func<Tuple<decimal, decimal>> getNationalMinimumWageRange, Func<decimal> getApprenticeNationalMinimumWage)
        {
            const int WeeksPerYear = 52;

            string wageText = "";

            switch (wage.WageType)
            {
                case WageType.FixedWage:
                    wageText = $"£{wage.FixedWageYearlyAmount?.AsMoney()}";
                    break;
                case WageType.NationalMinimumWage:
                    var hourlyRange = getNationalMinimumWageRange();

                    var minYearly = hourlyRange.Item1 * wage.WeeklyHours.Value * WeeksPerYear;
                    var maxYearly = hourlyRange.Item2 * wage.WeeklyHours.Value * WeeksPerYear;

                    wageText = $"£{minYearly.AsMoney()} - £{maxYearly.AsMoney()}";
                    break;
                case WageType.NationalMinimumWageForApprentices:
                    var hourly = getApprenticeNationalMinimumWage();

                    var yearly = hourly * wage.WeeklyHours.Value * WeeksPerYear;

                    wageText = $"£{yearly.AsMoney()}";
                    break;
                default:
                    wageText = wage.WageType?.GetDisplayName();
                    break;
            }

            return wageText;
        }

        public static string ToHoursPerWeekText(this Wage wage)
        {
            return wage.WeeklyHours.ToString().EndsWith("0")
                    ? wage.WeeklyHours.ToString().SkipLast(1).ToString().Replace(".0", string.Empty)
                    : wage.WeeklyHours.ToString();
        }
        
    }
}
