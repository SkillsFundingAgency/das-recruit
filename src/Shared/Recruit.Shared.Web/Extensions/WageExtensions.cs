using System;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Services.MinimumWage;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class WageExtensions
    {
        private const int WeeksPerYear = 52;

        public static string ToText(this Wage wage, Func<WageRange> getNationalMinimumWageRange, Func<decimal> getApprenticeNationalMinimumWage)
        {
            string wageText;

            switch (wage.WageType)
            {
                case WageType.FixedWage:
                    wageText = $"£{wage.FixedWageYearlyAmount?.AsMoney()}";
                    break;
                case WageType.NationalMinimumWage:
                    var hourlyRange = getNationalMinimumWageRange();

                    var minYearly = GetYearlyRateFromHourlyRate(hourlyRange.MinimumWage, wage.WeeklyHours.Value);
                    var maxYearly = GetYearlyRateFromHourlyRate(hourlyRange.MaximumWage, wage.WeeklyHours.Value);

                    wageText = $"£{minYearly.AsMoney()} - £{maxYearly.AsMoney()}";
                    break;
                case WageType.NationalMinimumWageForApprentices:
                    var hourlyRate = getApprenticeNationalMinimumWage();

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
