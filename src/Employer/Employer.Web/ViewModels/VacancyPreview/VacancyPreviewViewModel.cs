using System.Collections.Generic;

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
        public string SubmitButtonText { get; internal set; }



        public bool HasIncompleteSkillsSection => SkillsSectionState == VacancyPreviewSectionState.Incomplete;
        public bool HasIncompleteQualificationsSection => QualificationsSectionState == VacancyPreviewSectionState.Incomplete;
        public bool HasIncompleteEmployerDescriptionSection => EmployerDescriptionSectionState == VacancyPreviewSectionState.Incomplete;
        public bool HasIncompleteTrainingProviderSection => ProviderSectionState == VacancyPreviewSectionState.Incomplete;
        public bool HasIncompleteApplicationProcessSection => ApplicationMethodSectionState == VacancyPreviewSectionState.Incomplete;

        public bool HasIncompleteThingsToConsiderSection => ThingsToConsiderSectionState == VacancyPreviewSectionState.Incomplete;
        public bool HasIncompleteEmployerWebsiteUrlSection => EmployerWebsiteUrlSectionState == VacancyPreviewSectionState.Incomplete;
        public bool HasIncompleteContactSection => ContactSectionState == VacancyPreviewSectionState.Incomplete;


        public bool HasIncompleteMandatorySections => HasIncompleteVacancyDescription
                                                        || HasIncompleteSkillsSection
                                                        || HasIncompleteQualificationsSection
                                                        || HasIncompleteEmployerDescriptionSection
                                                        || HasIncompleteTrainingProviderSection
                                                        || HasIncompleteApplicationProcessSection;

        public bool HasIncompleteOptionalSections => HasIncompleteThingsToConsiderSection
                                                    || HasIncompleteEmployerWebsiteUrlSection
                                                    || HasIncompleteContactSection;

        public bool HasIncompleteSections => HasIncompleteMandatorySections || HasIncompleteOptionalSections;

        public bool DisplayReferredHeader { get; internal set; }
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
        
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
        Review
    }
}

