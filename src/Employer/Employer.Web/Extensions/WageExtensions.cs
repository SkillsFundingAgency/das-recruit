using Esfa.Recruit.Vacancies.Client.Domain.Enums;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    using Esfa.Recruit.Vacancies.Client.Domain.Entities;

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
    }
}
