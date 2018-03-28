using Esfa.Recruit.Vacancies.Client.Domain.Enums;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    using Esfa.Recruit.Vacancies.Client.Domain.Entities;
    using System.Linq;

    public static class WageExtensions
    {
        public static string ToText(this Wage wage)
        {
            if (wage.WageType == WageType.FixedWage)
            {
                return $"£ {wage.FixedWageYearlyAmount?.AsMoney()} yearly";
            }
            
            return wage.WageType?.GetDisplayName();
        }

        public static string ToHoursPerWeekText(this Wage wage)
        {
            return wage.WeeklyHours.ToString().EndsWith("0")
                    ? wage.WeeklyHours.ToString().SkipLast(1).ToString().Replace(".0", string.Empty)
                    : wage.WeeklyHours.ToString();
        }
    }
}
