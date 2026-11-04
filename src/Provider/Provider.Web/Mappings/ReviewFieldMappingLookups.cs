using System;
using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Dates;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Duration;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Location;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.MultipleLocations;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.NumberOfPositions;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Title;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Training;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Wage;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.AboutEmployer;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.ApplicationProcess;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.Considerations;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.FutureProspects;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.ProviderContactDetails;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.ShortDescription;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.VacancyDescription;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Services;

namespace Esfa.Recruit.Provider.Web.Mappings;

public static class ReviewFieldMappingLookups
{
    public static ReviewFieldMappingLookupsForPage GetPreviewReviewFieldIndicators()
    {
        var vms = new List<ReviewFieldIndicatorViewModel>
        {
            new(FieldIdentifiers.Title, Anchors.Title),
            new(FieldIdentifiers.ShortDescription, Anchors.ShortDescription),
            new(FieldIdentifiers.ClosingDate, Anchors.ClosingDate),
            new(FieldIdentifiers.WorkingWeek, Anchors.WorkingWeek),
            new(FieldIdentifiers.Wage, Anchors.YearlyWage),
            new(FieldIdentifiers.CompanyBenefitsInformation, Anchors.CompanyBenefitsInformation),
            new(FieldIdentifiers.ExpectedDuration, Anchors.ExpectedDuration),
            new(FieldIdentifiers.PossibleStartDate, Anchors.PossibleStartDate),
            new(FieldIdentifiers.TrainingLevel, Anchors.TrainingLevel),
            new(FieldIdentifiers.NumberOfPositions, Anchors.NumberOfPositions),
            new(FieldIdentifiers.VacancyDescription, Anchors.VacancyDescription),
            new(FieldIdentifiers.TrainingDescription, Anchors.TrainingDescription),
            new(FieldIdentifiers.AdditionalTrainingDescription, Anchors.AdditionalTrainingDescription),
            new(FieldIdentifiers.OutcomeDescription, Anchors.OutcomeDescription),
            new(FieldIdentifiers.Skills, Anchors.Skills),
            new(FieldIdentifiers.Qualifications, Anchors.Qualifications),
            new(FieldIdentifiers.HasOptedToAddQualifications, Anchors.HasOptedToAddQualifications),
            new(FieldIdentifiers.ThingsToConsider, Anchors.ThingsToConsider),
            new(FieldIdentifiers.EmployerDescription, Anchors.EmployerDescription),
            new(FieldIdentifiers.DisabilityConfident, Anchors.DisabilityConfident),
            new(FieldIdentifiers.EmployerWebsiteUrl, Anchors.EmployerWebsiteUrl),
            new(FieldIdentifiers.EmployerAddress, Anchors.EmployerAddress),
            new(FieldIdentifiers.EmployerLocations, Anchors.EmployerAddress),
            new(FieldIdentifiers.EmployerLocationInformation, Anchors.EmployerAddress),
            new(FieldIdentifiers.Provider, Anchors.Provider),
            new(FieldIdentifiers.ProviderContact, Anchors.ProviderContact),
            new(FieldIdentifiers.Training, Anchors.Training),
            new(FieldIdentifiers.ApplicationMethod, Anchors.ApplicationMethod),
            new(FieldIdentifiers.ApplicationUrl, Anchors.ApplicationUrl),
            new(FieldIdentifiers.ApplicationInstructions, Anchors.ApplicationInstructions),
            new(FieldIdentifiers.EmployerName, Anchors.AboutEmployerSection),
            new(FieldIdentifiers.AdditionalQuestion1, Anchors.AdditionalQuestion1),
            new(FieldIdentifiers.AdditionalQuestion2, Anchors.AdditionalQuestion2),
        };

        var mappings =  new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.EmployerAccountId), []},
            { FieldIdResolver.ToFieldId(v => v.Title), [FieldIdentifiers.Title]},
            { FieldIdResolver.ToFieldId(v => v.EmployerName), [FieldIdentifiers.EmployerName]},
            { FieldIdResolver.ToFieldId(v => v.ShortDescription), [FieldIdentifiers.ShortDescription]},
            { FieldIdResolver.ToFieldId(v => v.ClosingDate), [FieldIdentifiers.ClosingDate]},
            { FieldIdResolver.ToFieldId(v => v.Wage.WeeklyHours), [FieldIdentifiers.WorkingWeek]},
            { FieldIdResolver.ToFieldId(v => v.Wage.WorkingWeekDescription), [FieldIdentifiers.WorkingWeek]},
            { FieldIdResolver.ToFieldId(v => v.Wage.WageAdditionalInformation), [FieldIdentifiers.Wage]},
            { FieldIdResolver.ToFieldId(v => v.Wage.CompanyBenefitsInformation), [FieldIdentifiers.CompanyBenefitsInformation
                ]
            },
            { FieldIdResolver.ToFieldId(v => v.Wage.WageType), [FieldIdentifiers.Wage]},
            { FieldIdResolver.ToFieldId(v => v.Wage.FixedWageYearlyAmount), [FieldIdentifiers.Wage]},
            { FieldIdResolver.ToFieldId(v => v.Wage.Duration), [FieldIdentifiers.ExpectedDuration]},
            { FieldIdResolver.ToFieldId(v => v.Wage.DurationUnit), [FieldIdentifiers.ExpectedDuration]},
            { FieldIdResolver.ToFieldId(v => v.StartDate), [FieldIdentifiers.PossibleStartDate]},
            { FieldIdResolver.ToFieldId(v => v.ProgrammeId), [FieldIdentifiers.TrainingLevel, FieldIdentifiers.Training]
            },
            { FieldIdResolver.ToFieldId(v => v.VacancyReference), [] },
            { FieldIdResolver.ToFieldId(v => v.NumberOfPositions), [FieldIdentifiers.NumberOfPositions]},
            { FieldIdResolver.ToFieldId(v => v.Description), [FieldIdentifiers.VacancyDescription]},
            { FieldIdResolver.ToFieldId(v => v.TrainingDescription), [FieldIdentifiers.TrainingDescription]},
            { FieldIdResolver.ToFieldId(v => v.AdditionalTrainingDescription), [FieldIdentifiers.AdditionalTrainingDescription
                ]
            },
            { FieldIdResolver.ToFieldId(v => v.OutcomeDescription), [FieldIdentifiers.OutcomeDescription]},
            { FieldIdResolver.ToFieldId(v => v.Skills), [FieldIdentifiers.Skills]},
            { FieldIdResolver.ToFieldId(v => v.Qualifications), [FieldIdentifiers.Qualifications]},
            { FieldIdResolver.ToFieldId(v => v.HasOptedToAddQualifications), [FieldIdentifiers.HasOptedToAddQualifications
                ]
            },
            { FieldIdResolver.ToFieldId(v => v.ThingsToConsider), [FieldIdentifiers.ThingsToConsider]},
            { FieldIdResolver.ToFieldId(v => v.EmployerDescription), [FieldIdentifiers.EmployerDescription]},
            { FieldIdResolver.ToFieldId(v => v.DisabilityConfident), [FieldIdentifiers.DisabilityConfident]},
            { FieldIdResolver.ToFieldId(v => v.EmployerWebsiteUrl), [FieldIdentifiers.EmployerWebsiteUrl]},
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine1), [FieldIdentifiers.EmployerAddress]},
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine2), [FieldIdentifiers.EmployerAddress]},
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine3), [FieldIdentifiers.EmployerAddress]},
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine4), [FieldIdentifiers.EmployerAddress]},
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.Postcode), [FieldIdentifiers.EmployerAddress]},
            { FieldIdResolver.ToFieldId(v => v.EmployerLocations), [FieldIdentifiers.EmployerAddress]},
            { FieldIdResolver.ToFieldId(v => v.EmployerLocationInformation), [FieldIdentifiers.EmployerAddress]},
            { FieldIdResolver.ToFieldId(v => v.ProviderContact.Email), [FieldIdentifiers.ProviderContact]},
            { FieldIdResolver.ToFieldId(v => v.ProviderContact.Name), [FieldIdentifiers.ProviderContact]},
            { FieldIdResolver.ToFieldId(v => v.ProviderContact.Phone), [FieldIdentifiers.ProviderContact]},
            { FieldIdResolver.ToFieldId(v => v.ApplicationInstructions), [FieldIdentifiers.ApplicationInstructions]},
            { FieldIdResolver.ToFieldId(v => v.ApplicationMethod), [FieldIdentifiers.ApplicationMethod]},
            { FieldIdResolver.ToFieldId(v => v.ApplicationUrl), [FieldIdentifiers.ApplicationUrl]},
            { FieldIdResolver.ToFieldId(v => v.AdditionalQuestion1), [FieldIdentifiers.AdditionalQuestion1]},
            { FieldIdResolver.ToFieldId(v => v.AdditionalQuestion2), [FieldIdentifiers.AdditionalQuestion2]}
        };

        return new ReviewFieldMappingLookupsForPage(vms, mappings);
    }

    public static ReviewFieldMappingLookupsForPage GetEmployerNameReviewFieldIndicators()
    {
        var vms = new List<ReviewFieldIndicatorViewModel>
        {
            new(FieldIdentifiers.EmployerName, nameof(FieldIdentifiers.EmployerName))
        };

        var mappings =  new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.EmployerName), [FieldIdentifiers.EmployerName]}
        };

        return new ReviewFieldMappingLookupsForPage(vms, mappings);
    }

    public static ReviewFieldMappingLookupsForPage GetShortDescriptionReviewFieldIndicators()
    {
        var vms = new List<ReviewFieldIndicatorViewModel>
        {
            new(FieldIdentifiers.ShortDescription, nameof(ShortDescriptionEditModel.ShortDescription))
        };

        var mappings =  new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.ShortDescription), [FieldIdentifiers.ShortDescription]}
        };

        return new ReviewFieldMappingLookupsForPage(vms, mappings);
    }

    public static ReviewFieldMappingLookupsForPage GetTrainingReviewFieldIndicators()
    {
        var vms = new List<ReviewFieldIndicatorViewModel>
        {
            new(FieldIdentifiers.Training, nameof(TrainingEditModel.SelectedProgrammeId)),
            new(FieldIdentifiers.TrainingLevel, nameof(TrainingEditModel.SelectedProgrammeId))
        };

        var mappings =  new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.ClosingDate), [FieldIdentifiers.ClosingDate]},
            { FieldIdResolver.ToFieldId(v => v.StartDate), [FieldIdentifiers.PossibleStartDate]},
            { FieldIdResolver.ToFieldId(v => v.ProgrammeId), [FieldIdentifiers.TrainingLevel, FieldIdentifiers.Training]},
            { FieldIdResolver.ToFieldId(v => v.DisabilityConfident), [FieldIdentifiers.DisabilityConfident]}
        };

        return new ReviewFieldMappingLookupsForPage(vms, mappings);
    }

    public static ReviewFieldMappingLookupsForPage GetDatesReviewFieldIndicators()
    {
        var vms = new List<ReviewFieldIndicatorViewModel>
        {
            new(FieldIdentifiers.ClosingDate, nameof(DatesEditModel.ClosingDay)),
            new(FieldIdentifiers.PossibleStartDate, nameof(DatesEditModel.StartDay)),
            new(FieldIdentifiers.DisabilityConfident, nameof(DatesEditModel.IsDisabilityConfident))
        };

        var mappings = new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.ClosingDate), [FieldIdentifiers.ClosingDate]},
            { FieldIdResolver.ToFieldId(v => v.StartDate), [FieldIdentifiers.PossibleStartDate]},
            { FieldIdResolver.ToFieldId(v => v.DisabilityConfident), [FieldIdentifiers.DisabilityConfident]}
        };

        return new ReviewFieldMappingLookupsForPage(vms, mappings);
    }

    public static ReviewFieldMappingLookupsForPage GetDurationReviewFieldIndicators()
    {
        var vms = new List<ReviewFieldIndicatorViewModel>
        {
            new(FieldIdentifiers.ExpectedDuration, nameof(DurationEditModel.Duration)),
            new(FieldIdentifiers.WorkingWeek, nameof(DurationEditModel.WorkingWeekDescription))
        };

        var mappings = new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.Wage.WeeklyHours), [FieldIdentifiers.WorkingWeek]},
            { FieldIdResolver.ToFieldId(v => v.Wage.WorkingWeekDescription), [FieldIdentifiers.WorkingWeek]},
            { FieldIdResolver.ToFieldId(v => v.Wage.Duration), [FieldIdentifiers.ExpectedDuration]},
            { FieldIdResolver.ToFieldId(v => v.Wage.DurationUnit), [FieldIdentifiers.ExpectedDuration]}
        };

        return new ReviewFieldMappingLookupsForPage(vms, mappings);
    }

    public static ReviewFieldMappingLookupsForPage GetWageReviewFieldIndicators()
    {
        var vms = new List<ReviewFieldIndicatorViewModel>
        {
            new(FieldIdentifiers.Wage, Anchors.WageTypeHeading),
            new(FieldIdentifiers.WageAdditionalInfo, nameof(WageEditModel.WageAdditionalInformation)),
            new(FieldIdentifiers.CompanyBenefitsInformation, nameof(WageEditModel.CompanyBenefitsInformation))
        };

        var mappings =  new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.Wage.WeeklyHours), [FieldIdentifiers.WorkingWeek]},
            { FieldIdResolver.ToFieldId(v => v.Wage.WorkingWeekDescription), [FieldIdentifiers.WorkingWeek]},
            { FieldIdResolver.ToFieldId(v => v.Wage.WageAdditionalInformation), [FieldIdentifiers.WageAdditionalInfo]},
            { FieldIdResolver.ToFieldId(v => v.Wage.CompanyBenefitsInformation), [FieldIdentifiers.CompanyBenefitsInformation
                ]
            },
            { FieldIdResolver.ToFieldId(v => v.Wage.WageType), [FieldIdentifiers.Wage]},
            { FieldIdResolver.ToFieldId(v => v.Wage.FixedWageYearlyAmount), [FieldIdentifiers.Wage]},
            { FieldIdResolver.ToFieldId(v => v.Wage.Duration), [FieldIdentifiers.ExpectedDuration]},
            { FieldIdResolver.ToFieldId(v => v.Wage.DurationUnit), [FieldIdentifiers.ExpectedDuration]}
        };

        return new ReviewFieldMappingLookupsForPage(vms, mappings);
    }

    public static ReviewFieldMappingLookupsForPage GetTitleFieldIndicators()
    {
        var vms = new List<ReviewFieldIndicatorViewModel>
        {
            new(FieldIdentifiers.Title, nameof(TitleEditModel.Title))
        };

        var mappings =  new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.Title), [FieldIdentifiers.Title]}
        };

        return new ReviewFieldMappingLookupsForPage(vms, mappings);
    }

    public static ReviewFieldMappingLookupsForPage GetNumberOfPositionsFieldIndicators()
    {
        var vms = new List<ReviewFieldIndicatorViewModel>
        {
            new(FieldIdentifiers.NumberOfPositions, nameof(NumberOfPositionsEditModel.NumberOfPositions))
        };

        var mappings = new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.NumberOfPositions), [FieldIdentifiers.NumberOfPositions]}
        };

        return new ReviewFieldMappingLookupsForPage(vms, mappings);
    }

    public static ReviewFieldMappingLookupsForPage GetWhereIsApprenticeshipAvailableFieldIndicators()
    {
        var vms = new List<ReviewFieldIndicatorViewModel>
        {
            new (FieldIdentifiers.EmployerLocations, nameof(LocationAvailabilityViewModel.SelectedAvailability)),
            new (FieldIdentifiers.EmployerLocationInformation, nameof(LocationAvailabilityViewModel.SelectedAvailability)),
            new (FieldIdentifiers.EmployerAddress, nameof(LocationAvailabilityViewModel.SelectedAvailability)),//so that the validation code appears at the top.
            new (FieldIdentifiers.EmployerAddress1, nameof(LocationAvailabilityViewModel.SelectedAvailability)),
            new (FieldIdentifiers.EmployerAddress2, nameof(LocationAvailabilityViewModel.SelectedAvailability)),
            new (FieldIdentifiers.EmployerAddress3, nameof(LocationAvailabilityViewModel.SelectedAvailability)),
            new (FieldIdentifiers.EmployerAddress4, nameof(LocationAvailabilityViewModel.SelectedAvailability))
        };

        var mappings = new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine1), [FieldIdentifiers.EmployerAddress1] },
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine2), [FieldIdentifiers.EmployerAddress2] },
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine3), [FieldIdentifiers.EmployerAddress3] },
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine4), [FieldIdentifiers.EmployerAddress4] },
            { FieldIdResolver.ToFieldId(v => v.EmployerLocation.Postcode), [FieldIdentifiers.EmployerAddress] },
            { FieldIdResolver.ToFieldId(v => v.EmployerLocations), [FieldIdentifiers.EmployerAddress] },
            { FieldIdResolver.ToFieldId(v => v.EmployerLocationInformation), [FieldIdentifiers.EmployerAddress] },
        };

        return new ReviewFieldMappingLookupsForPage(vms, mappings);
    }

    public static ReviewFieldMappingLookupsForPage GetVacancyDescriptionFieldIndicators()
    {
        var vms = new List<ReviewFieldIndicatorViewModel>
        {
            new(FieldIdentifiers.VacancyDescription, nameof(VacancyDescriptionEditModel.VacancyDescription)),
            new(FieldIdentifiers.TrainingDescription, nameof(VacancyDescriptionEditModel.TrainingDescription)),

        };

        var mappings =  new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.Description), [FieldIdentifiers.VacancyDescription]},
            { FieldIdResolver.ToFieldId(v => v.TrainingDescription), [FieldIdentifiers.TrainingDescription]},
            { FieldIdResolver.ToFieldId(v => v.OutcomeDescription), [FieldIdentifiers.OutcomeDescription]}
        };

        return new ReviewFieldMappingLookupsForPage(vms, mappings);
    }

    public static ReviewFieldMappingLookupsForPage GetSkillsFieldIndicators()
    {
        var vms = new List<ReviewFieldIndicatorViewModel>
        {
            new(FieldIdentifiers.Skills, Anchors.SkillsHeading),
        };

        var mappings =  new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.Skills), [FieldIdentifiers.Skills]}
        };

        return new ReviewFieldMappingLookupsForPage(vms, mappings);
    }

    public static ReviewFieldMappingLookupsForPage GetQualificationsFieldIndicators()
    {
        var vms = new List<ReviewFieldIndicatorViewModel>
        {
            new(FieldIdentifiers.Qualifications, Anchors.QualificationsHeading)
        };

        var mappings =  new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.Qualifications), [FieldIdentifiers.Qualifications]}
        };

        return new ReviewFieldMappingLookupsForPage(vms, mappings);
    }

    public static ReviewFieldMappingLookupsForPage GetConsiderationsFieldIndicators()
    {
        var vms = new List<ReviewFieldIndicatorViewModel>
        {
            new(FieldIdentifiers.ThingsToConsider, nameof(ConsiderationsEditModel.ThingsToConsider))
        };

        var mappings =  new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.ThingsToConsider), [FieldIdentifiers.ThingsToConsider]}
        };

        return new ReviewFieldMappingLookupsForPage(vms, mappings);
    }

    public static ReviewFieldMappingLookupsForPage GetAboutEmployerFieldIndicators()
    {
        var vms = new List<ReviewFieldIndicatorViewModel>
        {
            new(FieldIdentifiers.EmployerDescription, nameof(AboutEmployerEditModel.EmployerDescription)),
            new(FieldIdentifiers.EmployerWebsiteUrl, nameof(AboutEmployerEditModel.EmployerWebsiteUrl))
        };

        var mappings =  new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.EmployerDescription), [FieldIdentifiers.EmployerDescription]},
            { FieldIdResolver.ToFieldId(v => v.EmployerWebsiteUrl), [FieldIdentifiers.EmployerWebsiteUrl]}
        };

        return new ReviewFieldMappingLookupsForPage(vms, mappings);
    }

    public static ReviewFieldMappingLookupsForPage GetProviderContactDetailsFieldIndicators()
    {
        var vms = new List<ReviewFieldIndicatorViewModel>
        {
            new(FieldIdentifiers.ProviderContact, nameof(ProviderContactDetailsEditModel.ProviderContactName)),
        };

        var mappings =  new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.ProviderContact.Email), [FieldIdentifiers.ProviderContact]},
            { FieldIdResolver.ToFieldId(v => v.ProviderContact.Name), [FieldIdentifiers.ProviderContact]},
            { FieldIdResolver.ToFieldId(v => v.ProviderContact.Phone), [FieldIdentifiers.ProviderContact]}
        };

        return new ReviewFieldMappingLookupsForPage(vms, mappings);
    }

    public static ReviewFieldMappingLookupsForPage GetApplicationProcessFieldIndicators()
    { 
        var vms = new List<ReviewFieldIndicatorViewModel>
        {
            new(FieldIdentifiers.ApplicationMethod, Anchors.ApplicationMethodHeading),
            new(FieldIdentifiers.ApplicationUrl, nameof(ApplicationProcessEditModel.ApplicationUrl)),
            new(FieldIdentifiers.ApplicationInstructions, nameof(ApplicationProcessEditModel.ApplicationInstructions))
        };

        var mappings = new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.ApplicationInstructions), [FieldIdentifiers.ApplicationInstructions]},
            { FieldIdResolver.ToFieldId(v => v.ApplicationMethod), [FieldIdentifiers.ApplicationMethod]},
            { FieldIdResolver.ToFieldId(v => v.ApplicationUrl), [FieldIdentifiers.ApplicationUrl]}
        };

        return new ReviewFieldMappingLookupsForPage(vms, mappings);
    }

    public static ReviewFieldMappingLookupsForPage GetFutureProspectsFieldIndicators()
    {
        var vms = new List<ReviewFieldIndicatorViewModel>
        {
            new(FieldIdentifiers.OutcomeDescription, nameof(FutureProspectsEditModel.FutureProspects))
        };

        var mappings = new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.OutcomeDescription), [FieldIdentifiers.OutcomeDescription]}
        };

        return new ReviewFieldMappingLookupsForPage(vms, mappings);
    }
        
    public static ReviewFieldMappingLookupsForPage GetAdditionalQuestionsFieldIndicators()
    {
        var vms = new List<ReviewFieldIndicatorViewModel>
        {
            new(FieldIdentifiers.AdditionalQuestion1, Anchors.AdditionalQuestion1),
            new(FieldIdentifiers.AdditionalQuestion2, Anchors.AdditionalQuestion2)
        };

        var mappings =  new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.AdditionalQuestion1), [FieldIdentifiers.AdditionalQuestion1]},
            { FieldIdResolver.ToFieldId(v => v.AdditionalQuestion2), [FieldIdentifiers.AdditionalQuestion2]}
        };

        return new ReviewFieldMappingLookupsForPage(vms, mappings);
    }
    public static ReviewFieldMappingLookupsForPage GetWhatWillTheApprenticeDoAtWorkDescriptionFieldIndicators()
    {
        var vms = new List<ReviewFieldIndicatorViewModel>
        {
            new(FieldIdentifiers.VacancyDescription, nameof(VacancyDescriptionEditModel.VacancyDescription)),
        };

        var mappings = new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.Description), [FieldIdentifiers.VacancyDescription]},
        };

        return new ReviewFieldMappingLookupsForPage(vms, mappings);
    }
    public static ReviewFieldMappingLookupsForPage GetTrainingDescriptionFieldIndicators()
    {
        var vms = new List<ReviewFieldIndicatorViewModel>
        {
            new(FieldIdentifiers.TrainingDescription, Anchors.TrainingDescription),
            new(FieldIdentifiers.AdditionalTrainingDescription, Anchors.AdditionalTrainingDescription)
        };

        var mappings = new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.TrainingDescription), [FieldIdentifiers.TrainingDescription]},
            { FieldIdResolver.ToFieldId(v => v.AdditionalTrainingDescription), [FieldIdentifiers.AdditionalTrainingDescription
                ]
            }
        };

        return new ReviewFieldMappingLookupsForPage(vms, mappings);
    }
        
    public static ReviewFieldMappingLookupsForPage GetAdditionalWageInformationFieldIndicators()
    {
        var vms = new List<ReviewFieldIndicatorViewModel>
        {
            new(FieldIdentifiers.WageAdditionalInfo, Anchors.WageAdditionalInformation),
            new(FieldIdentifiers.CompanyBenefitsInformation, Anchors.CompanyBenefitsInformation)
        };

        var mappings = new Dictionary<string, IEnumerable<string>>
        {
            { FieldIdResolver.ToFieldId(v => v.Wage.WageAdditionalInformation), [FieldIdentifiers.WageAdditionalInfo]},
            { FieldIdResolver.ToFieldId(v => v.Wage.CompanyBenefitsInformation), [FieldIdentifiers.CompanyBenefitsInformation
                ]
            }
        };

        return new ReviewFieldMappingLookupsForPage(vms, mappings);
    }
}