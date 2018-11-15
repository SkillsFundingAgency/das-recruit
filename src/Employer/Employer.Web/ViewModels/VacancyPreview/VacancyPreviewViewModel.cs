using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview
{
    public class VacancyPreviewViewModel : DisplayVacancyViewModel
    {
        public VacancyPreviewSectionState ApplicationInstructionsSectionState { get; internal set; }
        public VacancyPreviewSectionState ApplicationMethodSectionState { get; internal set; }
        public VacancyPreviewSectionState ApplicationUrlSectionState { get; internal set; }
        public VacancyPreviewSectionState ContactSectionState { get; internal set; }
        public VacancyPreviewSectionState ClosingDateSectionState { get; internal set; }
        public VacancyPreviewSectionState DisabilityConfidentSectionState { get; internal set; }
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
        
        public bool HasWage { get; internal set; }
        public bool HasProgramme { get; internal set; }

        public bool CanShowReference { get; set; }

        public bool HasIncompleteVacancyDescription => !HasVacancyDescription;
        public bool CanShowDraftHeader { get; internal set; }

        public bool HasIncompleteSkillsSection => SkillsSectionState == VacancyPreviewSectionState.Incomplete || SkillsSectionState == VacancyPreviewSectionState.InvalidIncomplete;
        public bool HasIncompleteQualificationsSection => QualificationsSectionState == VacancyPreviewSectionState.Incomplete || QualificationsSectionState == VacancyPreviewSectionState.InvalidIncomplete;
        public bool HasIncompleteEmployerDescriptionSection => EmployerDescriptionSectionState == VacancyPreviewSectionState.Incomplete || EmployerDescriptionSectionState == VacancyPreviewSectionState.InvalidIncomplete;
        public bool HasIncompleteTrainingProviderSection => ProviderSectionState == VacancyPreviewSectionState.Incomplete || ProviderSectionState == VacancyPreviewSectionState.InvalidIncomplete;
        public bool HasIncompleteApplicationProcessSection => ApplicationMethodSectionState == VacancyPreviewSectionState.Incomplete || ApplicationMethodSectionState == VacancyPreviewSectionState.InvalidIncomplete;

        public bool HasIncompleteThingsToConsiderSection => ThingsToConsiderSectionState == VacancyPreviewSectionState.Incomplete || ThingsToConsiderSectionState == VacancyPreviewSectionState.InvalidIncomplete;
        public bool HasIncompleteEmployerWebsiteUrlSection => EmployerWebsiteUrlSectionState == VacancyPreviewSectionState.Incomplete || EmployerWebsiteUrlSectionState == VacancyPreviewSectionState.InvalidIncomplete;
        public bool HasIncompleteContactSection => ContactSectionState == VacancyPreviewSectionState.Incomplete || ContactSectionState == VacancyPreviewSectionState.InvalidIncomplete;


        public bool HasIncompleteMandatorySections => HasIncompleteVacancyDescription
                                                        || HasIncompleteSkillsSection
                                                        || HasIncompleteQualificationsSection
                                                        || HasIncompleteEmployerDescriptionSection
                                                        || HasIncompleteTrainingProviderSection
                                                        || HasIncompleteApplicationProcessSection;

        public bool HasIncompleteOptionalSections => HasIncompleteThingsToConsiderSection
                                                    || HasIncompleteEmployerWebsiteUrlSection
                                                    || HasIncompleteContactSection;

        public bool ShowIncompleteSections => (HasIncompleteMandatorySections || HasIncompleteOptionalSections) && !Review.HasBeenReviewed;

        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public string SubmitButtonText => Review.HasBeenReviewed ? "Resubmit vacancy" : "Submit vacancy";

        public bool ApplicationInstructionsRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.ApplicationInstructions);
        public bool ApplicationMethodRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.ApplicationMethod);
        public bool ApplicationUrlRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.ApplicationUrl);
        public bool ClosingDateRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.ClosingDate);
        public bool EmployerContactRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.EmployerContact);
        public bool DisabilityConfidentRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.DisabilityConfident);
        public bool EmployerAddressRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.EmployerAddress);
        public bool EmployerDescriptionRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.EmployerDescription);
        public bool EmployerWebsiteUrlRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.EmployerWebsiteUrl);
        public bool ExpectedDurationRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.ExpectedDuration);
        public bool NumberOfPositionsRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.NumberOfPositions);
        public bool OutcomeDescriptionRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.OutcomeDescription);
        public bool PossibleStartDateRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.PossibleStartDate);
        public bool ProviderRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.Provider);
        public bool QualificationsRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.Qualifications);
        public bool ShortDescriptionRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.ShortDescription);
        public bool SkillsRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.Skills);
        public bool ThingsToConsiderRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.ThingsToConsider);
        public bool TitleRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.Title);
        public bool TrainingRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.Training);
        public bool TrainingDescriptionRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.TrainingDescription);
        public bool TrainingLevelRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.TrainingLevel);
        public bool VacancyDescriptionRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.VacancyDescription);
        public bool WageRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.Wage);
        public bool WorkingWeekRequiresEdit => IsEditRequired(VacancyReview.FieldIdentifiers.WorkingWeek);

        private bool IsEditRequired(string fieldIdentifier)
        {
            return Review.FieldIndicators.Any(f => f.ReviewFieldIdentifier == fieldIdentifier);
        }


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
            nameof(ContactName),
            nameof(ContactTelephone),
            nameof(ContactEmail),
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

