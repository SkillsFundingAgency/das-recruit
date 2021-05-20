using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue, UserType? userType = null)
        {
            if (enumValue == null)
            {
                return string.Empty;
            }

            DisplayNames.TryGetValue(enumValue, out var displayName);

            if (userType.HasValue)
            {
                switch (userType)
                {
                    case UserType.Employer:
                        if (DisplayNamesEmployer.TryGetValue(enumValue, out var displayNameEmployer))
                        {
                            displayName = displayNameEmployer;
                        }
                        break;

                    case UserType.Provider:
                        if (DisplayNamesProvider.TryGetValue(enumValue, out var displayNameProvider))
                        {
                            displayName = displayNameProvider;
                        }
                        break;
                }
            }

            return displayName ?? enumValue.ToString();
        }

        public static bool IsInLiveVacancyOptions(this FilteringOptions enumValue)
        {
            return enumValue == FilteringOptions.ClosingSoon ||
                   enumValue == FilteringOptions.ClosingSoonWithNoApplications;
        }

        private static readonly Dictionary<Enum, string> DisplayNames = new Dictionary<Enum, string>
        {
            { WageType.FixedWage, "Fixed wage" },
            { WageType.NationalMinimumWage, "National Minimum Wage" },
            { WageType.NationalMinimumWageForApprentices, "National Minimum Wage for apprentices" },
            { ManualQaOutcome.Referred, "Edits required" },
            { ReviewStatus.UnderReview, "Under review" },
            { VacancyStatus.Rejected, "Rejected" },
            { VacancyStatus.Referred, "Rejected" },
            { VacancyStatus.Review, "Pending employer review" },
            { VacancyStatus.Submitted, "Pending ESFA review" },                     
            { ApplicationReviewDisabilityStatus.PreferNotToSay, "Prefer not to say" },
            { FilteringOptions.ClosingSoon, "closing soon" },
            { FilteringOptions.ClosingSoonWithNoApplications, "closing soon without applications" },
            { FilteringOptions.AllApplications, "with applications" },
            { FilteringOptions.NewApplications, "with new applications" },
            { FilteringOptions.Review, "Pending employer review" },
            { FilteringOptions.Submitted, "Pending ESFA review" },
            { FilteringOptions.Referred, "Rejected" },
            { FilteringOptions.Transferred, "Transferred from provider" }
        };

        private static readonly Dictionary<Enum, string> DisplayNamesEmployer = new Dictionary<Enum, string>
        {
            { VacancyStatus.Review, "Ready for review" },
            { VacancyStatus.Submitted, "Pending review" },
            { FilteringOptions.Review, "Ready for review" },
            { FilteringOptions.Submitted, "Pending review" },
        };

        private static readonly Dictionary<Enum, string> DisplayNamesProvider = new Dictionary<Enum, string>
        {
            { FilteringOptions.Review, "Pending employer review" },
            { FilteringOptions.Submitted, "Pending ESFA review" },
        };
    }
}