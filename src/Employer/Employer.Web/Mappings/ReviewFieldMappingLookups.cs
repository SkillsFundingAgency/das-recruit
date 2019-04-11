using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Location;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.ShortDescription;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Title;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Training;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Services;

namespace Esfa.Recruit.Employer.Web.Mappings
{
    public static class ReviewFieldMappingLookups
    {
        public static ReviewFieldMappingLookupsForPage GetPreviewReviewFieldIndicators()
        {
            var vms = new List<ReviewFieldIndicatorViewModel>
            {
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.Title, Anchors.Title),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.ShortDescription, Anchors.ShortDescription),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.ClosingDate, Anchors.ClosingDate),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.WorkingWeek, Anchors.WorkingWeek),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.Wage, Anchors.YearlyWage),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.ExpectedDuration, Anchors.ExpectedDuration),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.PossibleStartDate, Anchors.PossibleStartDate),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.TrainingLevel, Anchors.TrainingLevel),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.NumberOfPositions, Anchors.NumberOfPositions),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.VacancyDescription, Anchors.VacancyDescription),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.TrainingDescription, Anchors.TrainingDescription),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.OutcomeDescription, Anchors.OutcomeDescription),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.Skills, Anchors.Skills),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.Qualifications, Anchors.Qualifications),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.ThingsToConsider, Anchors.ThingsToConsider),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerDescription, Anchors.EmployerDescription),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.DisabilityConfident, Anchors.DisabilityConfident),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerWebsiteUrl, Anchors.EmployerWebsiteUrl),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerContact, Anchors.EmployerContact),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerAddress, Anchors.EmployerAddress),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.Provider, Anchors.Provider),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.Training, Anchors.Training),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.ApplicationMethod, Anchors.ApplicationMethod),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.ApplicationUrl, Anchors.ApplicationUrl),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.ApplicationInstructions, Anchors.ApplicationInstructions),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerName, Anchors.AboutEmployerSection)
            };

            var mappings =  new Dictionary<string, IEnumerable<string>>
            {
                { FieldIdResolver.ToFieldId(v => v.EmployerAccountId), new string[0]},
                { FieldIdResolver.ToFieldId(v => v.Title), new[]{ FieldIdentifiers.Title} },
                { FieldIdResolver.ToFieldId(v => v.EmployerName), new []{ FieldIdentifiers.EmployerName} },
                { FieldIdResolver.ToFieldId(v => v.ShortDescription), new []{ FieldIdentifiers.ShortDescription} },
                { FieldIdResolver.ToFieldId(v => v.ClosingDate), new []{ FieldIdentifiers.ClosingDate} },
                { FieldIdResolver.ToFieldId(v => v.Wage.WeeklyHours), new []{ FieldIdentifiers.WorkingWeek} },
                { FieldIdResolver.ToFieldId(v => v.Wage.WorkingWeekDescription), new []{ FieldIdentifiers.WorkingWeek} },
                { FieldIdResolver.ToFieldId(v => v.Wage.WageAdditionalInformation), new []{ FieldIdentifiers.Wage}},
                { FieldIdResolver.ToFieldId(v => v.Wage.WageType),  new[]{ FieldIdentifiers.Wage}},
                { FieldIdResolver.ToFieldId(v => v.Wage.FixedWageYearlyAmount), new []{ FieldIdentifiers.Wage }},
                { FieldIdResolver.ToFieldId(v => v.Wage.Duration), new []{ FieldIdentifiers.ExpectedDuration }},
                { FieldIdResolver.ToFieldId(v => v.Wage.DurationUnit), new []{ FieldIdentifiers.ExpectedDuration }},
                { FieldIdResolver.ToFieldId(v => v.StartDate), new []{ FieldIdentifiers.PossibleStartDate} },
                { FieldIdResolver.ToFieldId(v => v.ProgrammeId), new []{ FieldIdentifiers.TrainingLevel, FieldIdentifiers.Training} },
                { FieldIdResolver.ToFieldId(v => v.VacancyReference), new string[0] },
                { FieldIdResolver.ToFieldId(v => v.NumberOfPositions), new []{ FieldIdentifiers.NumberOfPositions} },
                { FieldIdResolver.ToFieldId(v => v.Description), new []{ FieldIdentifiers.VacancyDescription} },
                { FieldIdResolver.ToFieldId(v => v.TrainingDescription), new []{ FieldIdentifiers.TrainingDescription} },
                { FieldIdResolver.ToFieldId(v => v.OutcomeDescription), new []{ FieldIdentifiers.OutcomeDescription} },
                { FieldIdResolver.ToFieldId(v => v.Skills), new []{ FieldIdentifiers.Skills} },
                { FieldIdResolver.ToFieldId(v => v.Qualifications), new []{ FieldIdentifiers.Qualifications} },
                { FieldIdResolver.ToFieldId(v => v.ThingsToConsider), new []{ FieldIdentifiers.ThingsToConsider} },
                { FieldIdResolver.ToFieldId(v => v.EmployerDescription), new [] { FieldIdentifiers.EmployerDescription }},
                { FieldIdResolver.ToFieldId(v => v.DisabilityConfident), new []{ FieldIdentifiers.DisabilityConfident} },
                { FieldIdResolver.ToFieldId(v => v.EmployerWebsiteUrl), new []{ FieldIdentifiers.EmployerWebsiteUrl} },
                { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine1), new []{ FieldIdentifiers.EmployerAddress }},
                { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine2), new []{ FieldIdentifiers.EmployerAddress }},
                { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine3), new []{ FieldIdentifiers.EmployerAddress }},
                { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine4), new []{ FieldIdentifiers.EmployerAddress }},
                { FieldIdResolver.ToFieldId(v => v.EmployerLocation.Postcode), new[] { FieldIdentifiers.EmployerAddress}},
                { FieldIdResolver.ToFieldId(v => v.EmployerContact.Email), new []{ FieldIdentifiers.EmployerContact} },
                { FieldIdResolver.ToFieldId(v => v.EmployerContact.Name), new [] { FieldIdentifiers.EmployerContact }},
                { FieldIdResolver.ToFieldId(v => v.EmployerContact.Phone), new []{ FieldIdentifiers.EmployerContact }},
                { FieldIdResolver.ToFieldId(v => v.TrainingProvider.Ukprn) , new []{ FieldIdentifiers.Provider} },
                { FieldIdResolver.ToFieldId(v => v.ApplicationInstructions), new [] { FieldIdentifiers.ApplicationInstructions }},
                { FieldIdResolver.ToFieldId(v => v.ApplicationMethod), new [] { FieldIdentifiers.ApplicationMethod} },
                { FieldIdResolver.ToFieldId(v => v.ApplicationUrl), new []{ FieldIdentifiers.ApplicationUrl} }
            };

            return new ReviewFieldMappingLookupsForPage(vms, mappings);
        }

        public static ReviewFieldMappingLookupsForPage GetEmployerNameReviewFieldIndicators()
        {
            var vms = new List<ReviewFieldIndicatorViewModel>
            {
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerName, nameof(FieldIdentifiers.EmployerName))
            };

            var mappings =  new Dictionary<string, IEnumerable<string>>
            {
                { FieldIdResolver.ToFieldId(v => v.EmployerName), new []{ FieldIdentifiers.EmployerName} }
            };

            return new ReviewFieldMappingLookupsForPage(vms, mappings);
        }

        public static ReviewFieldMappingLookupsForPage GetShortDescriptionReviewFieldIndicators()
        {
            var vms = new List<ReviewFieldIndicatorViewModel>
            {
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.ShortDescription, nameof(ShortDescriptionEditModel.ShortDescription))
            };

            var mappings =  new Dictionary<string, IEnumerable<string>>
            {
                { FieldIdResolver.ToFieldId(v => v.ShortDescription), new []{ FieldIdentifiers.ShortDescription} }
            };

            return new ReviewFieldMappingLookupsForPage(vms, mappings);
        }

        public static ReviewFieldMappingLookupsForPage GetTrainingReviewFieldIndicators()
        {
            var vms = new List<ReviewFieldIndicatorViewModel>
            {
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.ClosingDate, nameof(TrainingEditModel.ClosingDay)),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.PossibleStartDate, nameof(TrainingEditModel.StartDay)),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.Training, nameof(TrainingEditModel.SelectedProgrammeId)),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.TrainingLevel, nameof(TrainingEditModel.SelectedProgrammeId)),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.DisabilityConfident, nameof(TrainingEditModel.IsDisabilityConfident))
            };

            var mappings =  new Dictionary<string, IEnumerable<string>>
            {
                { FieldIdResolver.ToFieldId(v => v.ClosingDate), new []{ FieldIdentifiers.ClosingDate} },
                { FieldIdResolver.ToFieldId(v => v.StartDate), new []{ FieldIdentifiers.PossibleStartDate} },
                { FieldIdResolver.ToFieldId(v => v.ProgrammeId), new []{ FieldIdentifiers.TrainingLevel, FieldIdentifiers.Training} },
                { FieldIdResolver.ToFieldId(v => v.DisabilityConfident), new []{ FieldIdentifiers.DisabilityConfident} }
            };

            return new ReviewFieldMappingLookupsForPage(vms, mappings);
        }

        public static ReviewFieldMappingLookupsForPage GetWageReviewFieldIndicators()
        {
            var vms = new List<ReviewFieldIndicatorViewModel>
            {
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.ExpectedDuration, nameof(WageEditModel.Duration)),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.WorkingWeek, nameof(WageEditModel.WorkingWeekDescription)),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.Wage, Anchors.WageTypeHeading),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.WageAdditionalInfo, nameof(WageEditModel.WageAdditionalInformation))
            };

            var mappings =  new Dictionary<string, IEnumerable<string>>
            {
                { FieldIdResolver.ToFieldId(v => v.Wage.WeeklyHours), new []{ FieldIdentifiers.WorkingWeek} },
                { FieldIdResolver.ToFieldId(v => v.Wage.WorkingWeekDescription), new []{ FieldIdentifiers.WorkingWeek} },
                { FieldIdResolver.ToFieldId(v => v.Wage.WageAdditionalInformation), new []{ FieldIdentifiers.WageAdditionalInfo }},
                { FieldIdResolver.ToFieldId(v => v.Wage.WageType),  new[]{ FieldIdentifiers.Wage}},
                { FieldIdResolver.ToFieldId(v => v.Wage.FixedWageYearlyAmount), new []{ FieldIdentifiers.Wage }},
                { FieldIdResolver.ToFieldId(v => v.Wage.Duration), new []{ FieldIdentifiers.ExpectedDuration }},
                { FieldIdResolver.ToFieldId(v => v.Wage.DurationUnit), new []{ FieldIdentifiers.ExpectedDuration }},
            };

            return new ReviewFieldMappingLookupsForPage(vms, mappings);
        }

        public static ReviewFieldMappingLookupsForPage GetTitleFieldIndicators()
        {
            var vms = new List<ReviewFieldIndicatorViewModel>
            {
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.Title, nameof(TitleEditModel.Title)),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.NumberOfPositions, nameof(TitleEditModel.NumberOfPositions))
            };

            var mappings =  new Dictionary<string, IEnumerable<string>>
            {
                { FieldIdResolver.ToFieldId(v => v.Title), new[]{ FieldIdentifiers.Title} },
                { FieldIdResolver.ToFieldId(v => v.NumberOfPositions), new []{ FieldIdentifiers.NumberOfPositions} }
            };

            return new ReviewFieldMappingLookupsForPage(vms, mappings);
        }

        public static ReviewFieldMappingLookupsForPage GetLocationFieldIndicators()
        {
            var vms = new List<ReviewFieldIndicatorViewModel>
            {
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerAddress, nameof(LocationEditModel.AddressLine1)),//so that the validation code appears at the top.
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerAddress1, nameof(LocationEditModel.AddressLine1)),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerAddress2, nameof(LocationEditModel.AddressLine2)),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerAddress3, nameof(LocationEditModel.AddressLine3)),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerAddress4, nameof(LocationEditModel.AddressLine4))
            };

            var mappings =  new Dictionary<string, IEnumerable<string>>
            {
                { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine1), new []{ FieldIdentifiers.EmployerAddress1 }},
                { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine2), new []{ FieldIdentifiers.EmployerAddress2 }},
                { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine3), new []{ FieldIdentifiers.EmployerAddress3 }},
                { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine4), new []{ FieldIdentifiers.EmployerAddress4 }},
                { FieldIdResolver.ToFieldId(v => v.EmployerLocation.Postcode), new[]{ FieldIdentifiers.EmployerAddress}}
            };

            return new ReviewFieldMappingLookupsForPage(vms, mappings);
        }

        public static ReviewFieldMappingLookupsForPage GetVacancyDescriptionFieldIndicators()
        {
            var vms = new List<ReviewFieldIndicatorViewModel>
            {
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.VacancyDescription, nameof(VacancyDescriptionEditModel.VacancyDescription)),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.TrainingDescription, nameof(VacancyDescriptionEditModel.TrainingDescription)),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.OutcomeDescription, nameof(VacancyDescriptionEditModel.OutcomeDescription)),

            };

            var mappings =  new Dictionary<string, IEnumerable<string>>
            {
                { FieldIdResolver.ToFieldId(v => v.Description), new []{ FieldIdentifiers.VacancyDescription} },
                { FieldIdResolver.ToFieldId(v => v.TrainingDescription), new []{ FieldIdentifiers.TrainingDescription} },
                { FieldIdResolver.ToFieldId(v => v.OutcomeDescription), new []{ FieldIdentifiers.OutcomeDescription} }
            };

            return new ReviewFieldMappingLookupsForPage(vms, mappings);
        }

        public static ReviewFieldMappingLookupsForPage GetSkillsFieldIndicators()
        {
            var vms = new List<ReviewFieldIndicatorViewModel>
            {
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.Skills, Anchors.SkillsHeading),
            };

            var mappings =  new Dictionary<string, IEnumerable<string>>
            {
                { FieldIdResolver.ToFieldId(v => v.Skills), new []{ FieldIdentifiers.Skills} }
            };

            return new ReviewFieldMappingLookupsForPage(vms, mappings);
        }

        public static ReviewFieldMappingLookupsForPage GetQualificationsFieldIndicators()
        {
            var vms = new List<ReviewFieldIndicatorViewModel>
            {
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.Qualifications, Anchors.QualificationsHeading)
            };

            var mappings =  new Dictionary<string, IEnumerable<string>>
            {
                { FieldIdResolver.ToFieldId(v => v.Qualifications), new []{ FieldIdentifiers.Qualifications} }
            };

            return new ReviewFieldMappingLookupsForPage(vms, mappings);
        }

        public static ReviewFieldMappingLookupsForPage GetConsiderationsFieldIndicators()
        {
            var vms = new List<ReviewFieldIndicatorViewModel>
            {
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.ThingsToConsider, nameof(ConsiderationsEditModel.ThingsToConsider))
            };

            var mappings =  new Dictionary<string, IEnumerable<string>>
            {
                { FieldIdResolver.ToFieldId(v => v.ThingsToConsider), new []{ FieldIdentifiers.ThingsToConsider} }
            };

            return new ReviewFieldMappingLookupsForPage(vms, mappings);
        }

        public static ReviewFieldMappingLookupsForPage GetAboutEmployerFieldIndicators()
        {
            var vms = new List<ReviewFieldIndicatorViewModel>
            {
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerDescription, nameof(AboutEmployerEditModel.EmployerDescription)),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerWebsiteUrl, nameof(AboutEmployerEditModel.EmployerWebsiteUrl))
            };

            var mappings =  new Dictionary<string, IEnumerable<string>>
            {
                { FieldIdResolver.ToFieldId(v => v.EmployerDescription), new [] { FieldIdentifiers.EmployerDescription }},
                { FieldIdResolver.ToFieldId(v => v.EmployerWebsiteUrl), new []{ FieldIdentifiers.EmployerWebsiteUrl} }
            };

            return new ReviewFieldMappingLookupsForPage(vms, mappings);
        }

        public static ReviewFieldMappingLookupsForPage GetEmployerContactDetailsFieldIndicators()
        {
            var vms = new List<ReviewFieldIndicatorViewModel>
            {
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.EmployerContact, nameof(EmployerContactDetailsEditModel.EmployerContactName)),
            };

            var mappings =  new Dictionary<string, IEnumerable<string>>
            {
                { FieldIdResolver.ToFieldId(v => v.EmployerContact.Email), new []{ FieldIdentifiers.EmployerContact }},
                { FieldIdResolver.ToFieldId(v => v.EmployerContact.Name), new []{ FieldIdentifiers.EmployerContact }},
                { FieldIdResolver.ToFieldId(v => v.EmployerContact.Phone), new []{ FieldIdentifiers.EmployerContact }}
            };

            return new ReviewFieldMappingLookupsForPage(vms, mappings);
        }

        public static ReviewFieldMappingLookupsForPage GetTrainingProviderFieldIndicators()
        {   
            var vms = new List<ReviewFieldIndicatorViewModel>
            {
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.Provider, nameof(SelectTrainingProviderEditModel.Ukprn)),
            };

            var mappings =  new Dictionary<string, IEnumerable<string>>
            {
                { FieldIdResolver.ToFieldId(v => v.TrainingProvider.Ukprn) , new []{ FieldIdentifiers.Provider} }
            };

            return new ReviewFieldMappingLookupsForPage(vms, mappings);
        }

        public static ReviewFieldMappingLookupsForPage GetApplicationProcessFieldIndicators()
        { 
            var vms = new List<ReviewFieldIndicatorViewModel>
            {
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.ApplicationMethod, Anchors.ApplicationMethodHeading),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.ApplicationUrl, nameof(ApplicationProcessEditModel.ApplicationUrl)),
                new ReviewFieldIndicatorViewModel(FieldIdentifiers.ApplicationInstructions, nameof(ApplicationProcessEditModel.ApplicationInstructions))
            };

            var mappings = new Dictionary<string, IEnumerable<string>>
            {
                { FieldIdResolver.ToFieldId(v => v.ApplicationInstructions), new [] { FieldIdentifiers.ApplicationInstructions }},
                { FieldIdResolver.ToFieldId(v => v.ApplicationMethod), new [] { FieldIdentifiers.ApplicationMethod} },
                { FieldIdResolver.ToFieldId(v => v.ApplicationUrl), new []{ FieldIdentifiers.ApplicationUrl} }
            };

            return new ReviewFieldMappingLookupsForPage(vms, mappings);
        }
    }
}