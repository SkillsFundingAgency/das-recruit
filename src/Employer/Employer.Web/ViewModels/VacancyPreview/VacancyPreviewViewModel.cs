using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Humanizer;

namespace Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview
{
    public class VacancyPreviewViewModel : DisplayVacancyViewModel
    {
        public VacancyPreviewSectionState ApplicationInstructionsSectionState { get; internal set; }
        public VacancyPreviewSectionState ApplicationMethodSectionState { get; internal set; }
        public VacancyPreviewSectionState ApplicationUrlSectionState { get; internal set; }
        public VacancyPreviewSectionState ClosingDateSectionState { get; internal set; }
        public VacancyPreviewSectionState DisabilityConfidentSectionState { get; internal set; }
        public VacancyPreviewSectionState EmployerContactSectionState { get; internal set; }
        public VacancyPreviewSectionState EmployerDescriptionSectionState { get; internal set; }
        public VacancyPreviewSectionState EmployerNameSectionState { get; internal set; }
        public VacancyPreviewSectionState EmployerWebsiteUrlSectionState { get; internal set; }
        public VacancyPreviewSectionState ExpectedDurationSectionState { get; internal set; }
        public VacancyPreviewSectionState EmployerAddressSectionState { get; internal set; }
        public VacancyPreviewSectionState NumberOfPositionsSectionState { get; internal set;}
        public VacancyPreviewSectionState PossibleStartDateSectionState { get; internal set; }
        public VacancyPreviewSectionState ProviderSectionState { get; internal set; }
        public VacancyPreviewSectionState QualificationsSectionState { get; internal set; }
        public VacancyPreviewSectionState ShortDescriptionSectionState { get; internal set; }
        public VacancyPreviewSectionState SkillsSectionState { get; internal set; }
        public VacancyPreviewSectionState TitleSectionState { get; internal set; }
        public VacancyPreviewSectionState ThingsToConsiderSectionState { get; internal set; }
        public VacancyPreviewSectionState TrainingSectionState { get; internal set; }
        public VacancyPreviewSectionState TrainingLevelSectionState { get; internal set; }
        public VacancyPreviewSectionState WageTextSectionState { get; internal set; }
        public VacancyPreviewSectionState DescriptionsSectionState { get; internal set; }
        public VacancyPreviewSectionState WorkingWeekSectionState { get; internal set; }

        public EntityValidationResult SoftValidationErrors { get; internal set; }
        public bool CanHideValidationSummary { get; internal set; }

        public bool HasWage { get; internal set; }
        public bool HasProgramme { get; internal set; }

        public bool CanShowReference { get; set; }
        public bool HasIncompleteVacancyDescription => !HasVacancyDescription;
        public bool HasIncompleteShortDescription => !HasShortDescription;
        public bool CanShowDraftHeader { get; internal set; }
        public bool HasIncompleteSkillsSection => SkillsSectionState == VacancyPreviewSectionState.Incomplete || SkillsSectionState == VacancyPreviewSectionState.InvalidIncomplete;
        public bool HasIncompleteQualificationsSection => QualificationsSectionState == VacancyPreviewSectionState.Incomplete || QualificationsSectionState == VacancyPreviewSectionState.InvalidIncomplete;
        public bool HasIncompleteEmployerDescriptionSection => EmployerDescriptionSectionState == VacancyPreviewSectionState.Incomplete || EmployerDescriptionSectionState == VacancyPreviewSectionState.InvalidIncomplete;
        public bool HasIncompleteTrainingProviderSection => ProviderSectionState == VacancyPreviewSectionState.Incomplete || ProviderSectionState == VacancyPreviewSectionState.InvalidIncomplete;
        public bool HasIncompleteApplicationProcessSection => ApplicationMethodSectionState == VacancyPreviewSectionState.Incomplete || ApplicationMethodSectionState == VacancyPreviewSectionState.InvalidIncomplete;
        public bool HasIncompleteThingsToConsiderSection => ThingsToConsiderSectionState == VacancyPreviewSectionState.Incomplete || ThingsToConsiderSectionState == VacancyPreviewSectionState.InvalidIncomplete;
        public bool HasIncompleteEmployerWebsiteUrlSection => EmployerWebsiteUrlSectionState == VacancyPreviewSectionState.Incomplete || EmployerWebsiteUrlSectionState == VacancyPreviewSectionState.InvalidIncomplete;
        public bool HasIncompleteContactSection => EmployerContactSectionState == VacancyPreviewSectionState.Incomplete || EmployerContactSectionState == VacancyPreviewSectionState.InvalidIncomplete;
        public bool HasIncompleteMandatorySections => HasIncompleteShortDescription
                                                        || HasIncompleteVacancyDescription
                                                        || HasIncompleteSkillsSection
                                                        || HasIncompleteQualificationsSection
                                                        || HasIncompleteEmployerDescriptionSection
                                                        || HasIncompleteTrainingProviderSection
                                                        || HasIncompleteApplicationProcessSection;
        public bool HasIncompleteOptionalSections => HasIncompleteThingsToConsiderSection
                                                    || HasIncompleteEmployerWebsiteUrlSection
                                                    || HasIncompleteContactSection;

        public bool HasSoftValidationErrors => SoftValidationErrors?.HasErrors == true;

        public bool ShowIncompleteSections => ((HasIncompleteMandatorySections || HasIncompleteOptionalSections) && !Review.HasBeenReviewed) || HasSoftValidationErrors;
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
        public string SubmitButtonText => Review.HasBeenReviewed ? "Resubmit vacancy" : "Submit vacancy";
        public bool ApplicationInstructionsRequiresEdit => IsEditRequired(FieldIdentifiers.ApplicationInstructions);
        public bool ApplicationMethodRequiresEdit => IsEditRequired(FieldIdentifiers.ApplicationMethod);
        public bool ApplicationUrlRequiresEdit => IsEditRequired(FieldIdentifiers.ApplicationUrl);
        public bool ClosingDateRequiresEdit => IsEditRequired(FieldIdentifiers.ClosingDate);
        public bool EmployerContactRequiresEdit => IsEditRequired(FieldIdentifiers.EmployerContact);
        public bool EmployerNameRequiresEdit => IsEditRequired(FieldIdentifiers.EmployerName);
        public bool DisabilityConfidentRequiresEdit => IsEditRequired(FieldIdentifiers.DisabilityConfident);
        public bool EmployerAddressRequiresEdit => IsEditRequired(FieldIdentifiers.EmployerAddress);
        public bool EmployerDescriptionRequiresEdit => IsEditRequired(FieldIdentifiers.EmployerDescription);
        public bool EmployerWebsiteUrlRequiresEdit => IsEditRequired(FieldIdentifiers.EmployerWebsiteUrl);
        public bool ExpectedDurationRequiresEdit => IsEditRequired(FieldIdentifiers.ExpectedDuration);
        public bool NumberOfPositionsRequiresEdit => IsEditRequired(FieldIdentifiers.NumberOfPositions);
        public bool OutcomeDescriptionRequiresEdit => IsEditRequired(FieldIdentifiers.OutcomeDescription);
        public bool PossibleStartDateRequiresEdit => IsEditRequired(FieldIdentifiers.PossibleStartDate);
        public bool ProviderRequiresEdit => IsEditRequired(FieldIdentifiers.Provider);
        public bool QualificationsRequiresEdit => IsEditRequired(FieldIdentifiers.Qualifications);
        public bool ShortDescriptionRequiresEdit => IsEditRequired(FieldIdentifiers.ShortDescription);
        public bool SkillsRequiresEdit => IsEditRequired(FieldIdentifiers.Skills);
        public bool ThingsToConsiderRequiresEdit => IsEditRequired(FieldIdentifiers.ThingsToConsider);
        public bool TitleRequiresEdit => IsEditRequired(FieldIdentifiers.Title);
        public bool TrainingRequiresEdit => IsEditRequired(FieldIdentifiers.Training);
        public bool TrainingDescriptionRequiresEdit => IsEditRequired(FieldIdentifiers.TrainingDescription);
        public bool TrainingLevelRequiresEdit => IsEditRequired(FieldIdentifiers.TrainingLevel);
        public bool VacancyDescriptionRequiresEdit => IsEditRequired(FieldIdentifiers.VacancyDescription);
        public bool WageRequiresEdit => IsEditRequired(FieldIdentifiers.Wage);
        public bool WorkingWeekRequiresEdit => IsEditRequired(FieldIdentifiers.WorkingWeek);

        private bool IsEditRequired(string fieldIdentifier)
        {
            return Review.FieldIndicators.Any(f => f.ReviewFieldIdentifier == fieldIdentifier);
        }

        public int IncompleteRequiredSectionCount => new []
                {
                    ShortDescriptionSectionState,
                    SkillsSectionState,
                    DescriptionsSectionState,
                    QualificationsSectionState,
                    EmployerDescriptionSectionState,
                    ProviderSectionState,
                    TrainingSectionState,
                    ApplicationMethodSectionState
                }.Count(s => s == VacancyPreviewSectionState.Incomplete || s == VacancyPreviewSectionState.InvalidIncomplete);

        public string IncompleteRequiredSectionText => "section".ToQuantity(IncompleteRequiredSectionCount, ShowQuantityAs.None);
        public ApprenticeshipLevel Level { get; set; }

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(ShortDescription),
            nameof(ClosingDate),
            nameof(WorkingWeekDescription),
            nameof(HasWage),
            nameof(WorkingWeekDescription),
            nameof(HoursPerWeek),
            nameof(WageText),
            nameof(WageInfo),
            nameof(ExpectedDuration),
            nameof(PossibleStartDate),
            nameof(HasProgramme),
            nameof(TrainingLevel),
            nameof(NumberOfPositions),
            nameof(VacancyDescription),
            nameof(TrainingDescription),
            nameof(OutcomeDescription),
            nameof(Skills),
            nameof(Qualifications),
            nameof(ThingsToConsider),
            nameof(EmployerDescription),
            nameof(EmployerName),
            nameof(EmployerWebsiteUrl),
            nameof(EmployerContactName),
            nameof(EmployerContactEmail),
            nameof(EmployerContactTelephone),
            nameof(EmployerAddressElements),
            nameof(ApplicationMethod),
            nameof(ApplicationInstructions),
            nameof(ApplicationUrl),
            nameof(ProviderName),
            nameof(TrainingType),
            nameof(TrainingTitle)
        };
    }

    public enum VacancyPreviewSectionState
    {
        Incomplete,
        Valid,
        Invalid,
        InvalidIncomplete,
        Review
    }
}

