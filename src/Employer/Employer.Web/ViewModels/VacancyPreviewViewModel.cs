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
        public string ApplicationUrl { get; internal set; }
        public bool CanDelete { get; internal set; }
        public bool CanSubmit { get; internal set; }
        public string ContactName { get; internal set; }
        public string ContactEmail { get; internal set; }
        public string ContactTelephone { get; internal set; }
        public string ClosingDate { get; internal set; }
        public VacancyPreviewSectionState ClosingDateSectionState { get; internal set; }
        public string EmployerDescription { get; internal set; }
        public string EmployerName { get; internal set; }
        public string EmployerWebsiteUrl { get; internal set; }
        public string ExpectedDuration { get; internal set; }
        public VacancyPreviewSectionState ExpectedDurationSectionState { get; internal set; }
        public string HoursPerWeek { get; internal set; }
        public IEnumerable<string> EmployerAddressElements { get; internal set; }
        public string MapUrl { get; set; }
        public string NumberOfPositions { get; internal set; }
        public VacancyPreviewSectionState NumberOfPositionsSectionState { get; internal set;}
        public string NumberOfPositionsCaption { get; internal set; }
        public string OutcomeDescription { get; internal set; }
        public string PossibleStartDate { get; internal set; }
        public VacancyPreviewSectionState PossibleStartDateSectionState { get; internal set; }
        public string ProviderName { get; internal set; }
        public string ProviderAddress { get; internal set; }
        public List<string> Qualifications { get; internal set; }
        public VacancyPreviewSectionState QualificationsSectionState { get; internal set; }
        public string ShortDescription { get; internal set; }
        public VacancyPreviewSectionState ShortDescriptionSectionState { get; internal set; }
        public IEnumerable<string> Skills { get; internal set; }
        public VacancyPreviewSectionState SkillsSectionState { get; internal set; }
        public string ThingsToConsider { get; internal set; }
        public string Title { get; internal set; }
        public string TrainingDescription { get; internal set; }
        public string TrainingTitle { get; internal set; }
        public string TrainingType { get; internal set; }
        public string TrainingLevel { get; internal set; }
        public VacancyPreviewSectionState TrainingLevelSectionState { get; internal set; }
        public string VacancyDescription { get; internal set; }
        public string VacancyReferenceNumber { get; internal set; }
        public string WageInfo { get; internal set; }
        public string WageText { get; internal set; }
        public VacancyPreviewSectionState WageTextSectionState { get; internal set; }
        public string WorkingWeekDescription { get; internal set; }

        public bool HasMapUrl => !string.IsNullOrEmpty(MapUrl);

        public bool HasTrainingProviderDetails => !string.IsNullOrEmpty(ProviderName);

        public bool HasContactDetails =>    !string.IsNullOrEmpty(ContactName) 
                                            && !string.IsNullOrEmpty(ContactEmail)
                                            && !string.IsNullOrEmpty(ContactTelephone);

        public VacancyPreviewSectionState DescriptionsSectionState { get; internal set; }

        public VacancyPreviewSectionState WorkingWeekSectionState { get; internal set; }

        [BindNever]
        public bool Wage { get; }
        [BindNever]
        public bool Programme{get;} 

        public IList<string> OrderedFieldNames => new List<string>
        {
            // This list is incomplete
            nameof(ShortDescription),
            nameof(ClosingDate),
            nameof(WorkingWeekDescription),
            nameof(Wage),
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
            nameof(Skills)
        };
    }

    public enum VacancyPreviewSectionState
    {
        Incomplete,
        Valid,
        Invalid
    }
}

