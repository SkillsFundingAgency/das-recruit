using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;

namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class WageExtensions
    {
        private const int WeeksPerYear = 52;

        public static string ToText(this Wage wage, IMinimumWage wagePeriod)
        {
            string wageText;

            switch (wage.WageType)
            {
                case WageType.FixedWage:
                    wageText = $"£{wage.FixedWageYearlyAmount?.AsMoney()}";
                    break;
                case WageType.NationalMinimumWage:

                    var minYearly = GetYearlyRateFromHourlyRate(wagePeriod.NationalMinimumWageLowerBound, wage.WeeklyHours.Value);
                    var maxYearly = GetYearlyRateFromHourlyRate(wagePeriod.NationalMinimumWageUpperBound, wage.WeeklyHours.Value);

                    wageText = $"£{minYearly.AsMoney()} - £{maxYearly.AsMoney()}";
                    break;
                case WageType.NationalMinimumWageForApprentices:
                    var hourlyRate = wagePeriod.ApprenticeshipMinimumWage;

                    var yearly = GetYearlyRateFromHourlyRate(hourlyRate, wage.WeeklyHours.Value);

                    wageText = $"£{yearly.AsMoney()}";
                    break;
                default:
                    wageText = wage.WageType?.GetDisplayName();
                    break;
            }

            return wageText;
        }

        private static decimal GetYearlyRateFromHourlyRate(decimal hourlyRate, decimal weeklyHours)
        {
            var yearlyRate = hourlyRate * weeklyHours * WeeksPerYear;
            return decimal.Round(yearlyRate, 2, MidpointRounding.AwayFromZero);
        }
    }
}
