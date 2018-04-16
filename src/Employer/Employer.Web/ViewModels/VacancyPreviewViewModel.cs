using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Humanizer;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class VacancyPreviewViewModel
    {
        public string ApplicationInstructions { get; internal set; }
        public VacancyPreviewSectionState ApplicationInstructionsSectionState { get; internal set; }
        public string ApplicationUrl { get; internal set; }
        public VacancyPreviewSectionState ApplicationUrlSectionState { get; internal set; }
        public bool CanDelete { get; internal set; }
        public bool CanSubmit { get; internal set; }
        public string ContactName { get; internal set; }
        public string ContactEmail { get; internal set; }
        public string ContactTelephone { get; internal set; }
        public VacancyPreviewSectionState ContactSectionState { get; internal set; }
        public string ClosingDate { get; internal set; }
        public VacancyPreviewSectionState ClosingDateSectionState { get; internal set; }
        public string EmployerDescription { get; internal set; }
        public VacancyPreviewSectionState EmployerDescriptionSectionState { get; internal set; }
        public string EmployerName { get; internal set; }
        public VacancyPreviewSectionState EmployerNameSectionState { get; internal set; }
        public string EmployerWebsiteUrl { get; internal set; }
        public VacancyPreviewSectionState EmployerWebsiteUrlSectionState { get; internal set; }
        public string ExpectedDuration { get; internal set; }
        public VacancyPreviewSectionState ExpectedDurationSectionState { get; internal set; }
        public string HoursPerWeek { get; internal set; }
        public IEnumerable<string> EmployerAddressElements { get; internal set; }
        public VacancyPreviewSectionState EmployerAddressSectionState { get; internal set; }
        public string MapUrl { get; set; }
        public string NumberOfPositions { get; internal set; }
        public VacancyPreviewSectionState NumberOfPositionsSectionState { get; internal set;}
        public string NumberOfPositionsCaption { get; internal set; }
        public string OutcomeDescription { get; internal set; }
        public string PossibleStartDate { get; internal set; }
        public VacancyPreviewSectionState PossibleStartDateSectionState { get; internal set; }
        public string ProviderName { get; internal set; }
        public VacancyPreviewSectionState ProviderSectionState { get; internal set; }
        public List<string> Qualifications { get; internal set; }
        public VacancyPreviewSectionState QualificationsSectionState { get; internal set; }
        public string ShortDescription { get; internal set; }
        public VacancyPreviewSectionState ShortDescriptionSectionState { get; internal set; }
        public IEnumerable<string> Skills { get; internal set; }
        public VacancyPreviewSectionState SkillsSectionState { get; internal set; }
        public string ThingsToConsider { get; internal set; }
        public VacancyPreviewSectionState ThingsToConsiderSectionState { get; internal set; }
        public string Title { get; internal set; }
        public string TrainingDescription { get; internal set; }
        public string TrainingTitle { get; internal set; }
        public string TrainingType { get; internal set; }
        public string TrainingLevel { get; internal set; }
        public VacancyPreviewSectionState TrainingSectionState { get; internal set; }
        public VacancyPreviewSectionState TrainingLevelSectionState { get; internal set; }
        public string VacancyDescription { get; internal set; }
        public string VacancyReferenceNumber { get; internal set; }
        public string WageInfo { get; internal set; }
        public string WageText { get; internal set; }
        public VacancyPreviewSectionState WageTextSectionState { get; internal set; }
        public string WorkingWeekDescription { get; internal set; }

        public VacancyPreviewSectionState DescriptionsSectionState { get; internal set; }

        public VacancyPreviewSectionState WorkingWeekSectionState { get; internal set; }

        public bool HasClosingDate => !string.IsNullOrWhiteSpace(ClosingDate);
        
        public bool HasShortDescription => !string.IsNullOrWhiteSpace(ShortDescription);

        public bool HasProviderName => !string.IsNullOrWhiteSpace(ProviderName);

        public bool HasTrainingTitle => !string.IsNullOrWhiteSpace(TrainingTitle);
        
        public bool HasWorkingWeekDescription => !string.IsNullOrWhiteSpace(WorkingWeekDescription);

        public bool HasHoursPerWeek => !string.IsNullOrWhiteSpace(HoursPerWeek);

        public bool HasWageText => !string.IsNullOrWhiteSpace(WageText);

        public bool HasWageInfo => !string.IsNullOrWhiteSpace(WageInfo);

        public bool HasExpectedDuration => !string.IsNullOrWhiteSpace(ExpectedDuration);

        public bool HasPossibleStartDate => !string.IsNullOrWhiteSpace(PossibleStartDate);

        public bool HasTrainingLevel => !string.IsNullOrWhiteSpace(TrainingLevel);

        public bool HasNumberOfPositionsCaption => !string.IsNullOrWhiteSpace(NumberOfPositionsCaption);
        public bool HasMapUrl => !string.IsNullOrEmpty(MapUrl);

        public bool HasVacancyDescription => !string.IsNullOrWhiteSpace(VacancyDescription);

        public bool HasTrainingDescription => !string.IsNullOrWhiteSpace(TrainingDescription);

        public bool HasOutcomeDescription => !string.IsNullOrWhiteSpace(OutcomeDescription);

        public bool HasSkills => Skills == null || Skills.Any();

        public bool HasQualifications => Qualifications != null && Qualifications.Any();

        public bool HasThingsToConsider => !string.IsNullOrWhiteSpace(ThingsToConsider);

        public bool HasEmployerName => !string.IsNullOrWhiteSpace(EmployerName);

        public bool HasEmployerDescription => !string.IsNullOrWhiteSpace(EmployerDescription);

        public bool HasEmployerWebsiteUrl => !string.IsNullOrWhiteSpace(EmployerWebsiteUrl);

        public bool HasContactName => !string.IsNullOrWhiteSpace(ContactName);

        public bool HasContactEmail => !string.IsNullOrWhiteSpace(ContactEmail);

        public bool HasContactTelephone => !string.IsNullOrWhiteSpace(ContactTelephone);

        public bool HasEmployerAddressElements => EmployerAddressElements != null && EmployerAddressElements.Any();

        public bool HasApplicationInstructions => !string.IsNullOrWhiteSpace(ApplicationInstructions);

        public bool HasApplicationUrl => !string.IsNullOrWhiteSpace(ApplicationUrl);
        
        public bool Wage { get; internal set; }
        
        public bool Programme { get; internal set; }

        public IList<string> OrderedFieldNames => new List<string>
        {
            // This list is incomplete
            nameof(ShortDescription),
            nameof(ClosingDate),
            nameof(WorkingWeekDescription),
            nameof(Wage),
            nameof(WorkingWeekDescription),
            nameof(HoursPerWeek),
            nameof(WageText),
            nameof(WageInfo),
            nameof(ExpectedDuration),
            nameof(PossibleStartDate),
            nameof(Programme),
            nameof(TrainingLevel),
            nameof(NumberOfPositions),
            nameof(VacancyDescription),
            nameof(TrainingDescription),
            nameof(OutcomeDescription),
            nameof(Skills),
            nameof(Qualifications),
            nameof(ThingsToConsider),
            nameof(EmployerName),
            nameof(EmployerDescription),
            nameof(EmployerWebsiteUrl),
            nameof(ContactName),
            nameof(ContactTelephone),
            nameof(ContactEmail),
            nameof(EmployerAddressElements),
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
        Invalid
    }
}

