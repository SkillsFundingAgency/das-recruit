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

        private static readonly Dictionary<Enum, string> DisplayNames = new()
        {
            { WageType.FixedWage, "Fixed wage" },
            { WageType.NationalMinimumWage, "National Minimum Wage" },
            { WageType.NationalMinimumWageForApprentices, "National Minimum Wage for apprentices" },
            { WageType.CompetitiveSalary, "This pay rate is above National Minimum Wage." },
            { ManualQaOutcome.Referred, "Edits required" },
            { ReviewStatus.UnderReview, "Under review" },
            { VacancyStatus.Rejected, "Rejected by employer" },
            { VacancyStatus.Referred, "Rejected by DfE" },
            { VacancyStatus.Review, "Pending employer review" },
            { VacancyStatus.Submitted, "Pending DfE review" },                     
            { ApplicationReviewDisabilityStatus.PreferNotToSay, "Prefer not to say" },
            { FilteringOptions.ClosingSoon, "closing soon" },
            { FilteringOptions.ClosingSoonWithNoApplications, "closing soon without applications" },
            { FilteringOptions.AllApplications, "with applications" },
            { FilteringOptions.NewApplications, "with new applications" },
            { FilteringOptions.NewSharedApplications, "with new shared applications" },
            { FilteringOptions.AllSharedApplications, "with shared applications" },
            { FilteringOptions.Review, "Pending employer review" },
            { FilteringOptions.Submitted, "Pending DfE review" },
            { FilteringOptions.Referred, "Rejected" },
            { FilteringOptions.Transferred, "Transferred from provider" },
            { FilteringOptions.EmployerReviewedApplications, "with employer-reviewed applications" },
            { QualificationWeighting.Desired, "Desirable" },
            { ApplicationReviewStatus.InReview, "In review" },
            { TrainingType.Standard, "Apprenticeship standard" },
            { TrainingType.Foundation, "Foundation apprenticeship" },
        };

        private static readonly Dictionary<Enum, string> DisplayNamesEmployer = new()
        {
            { VacancyStatus.Review, "Ready for review" },
            { VacancyStatus.Submitted, "Pending review" },
            { FilteringOptions.Review, "Ready for review" },
            { FilteringOptions.Submitted, "Pending review" },
            { ApplicationReviewStatus.EmployerInterviewing, "Interviewing" },
            { ApplicationReviewStatus.Shared, "Response Needed" },
            { ApplicationReviewStatus.EmployerUnsuccessful, "Unsuccessful" }
        };

        private static readonly Dictionary<Enum, string> DisplayNamesProvider = new()
        {
            { FilteringOptions.Review, "Pending employer review" },
            { FilteringOptions.Submitted, "Pending DfE review" },
            { ApplicationReviewStatus.EmployerInterviewing, "Employer reviewed" },
            { ApplicationReviewStatus.EmployerUnsuccessful, "Employer reviewed" }
        };
    }
}