using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            if (enumValue == null)
            {
                return string.Empty;
            }

            DisplayNames.TryGetValue(enumValue, out var displayName);
            return displayName ?? enumValue.ToString();
        }

        private static readonly Dictionary<Enum, string> DisplayNames = new Dictionary<Enum, string>
        {
            {ProgrammeLevel.FoundationDegree, "Foundation Degree" },
            {ProgrammeLevel.Masters, "Master's Degree" },
            {WageType.FixedWage, "Fixed wage" },
            {WageType.NationalMinimumWage, "National Minimum Wage" },
            {WageType.NationalMinimumWageForApprentices, "National Minimum Wage for apprentices" },
        };
    }
}
