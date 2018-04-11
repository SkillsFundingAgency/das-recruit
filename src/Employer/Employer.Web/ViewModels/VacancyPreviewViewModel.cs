using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Humanizer;

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
        public string EmployerDescription { get; internal set; }
        public string EmployerName { get; internal set; }
        public string EmployerWebsiteUrl { get; internal set; }
        public string ExpectedDuration { get; internal set; }
        public string HoursPerWeek { get; internal set; }
        public Address Location { get; internal set; }
        public IEnumerable<string> EmployerAddressElements => new[] { Location.AddressLine1, Location.AddressLine2, Location.AddressLine3, Location.AddressLine4, Location.Postcode }
                                                                .Where(x => !string.IsNullOrEmpty(x));
        public string MapUrl { get; set; }
        public int NumberOfPositions { get; internal set; }
        public string NumberOfPositionsCaption => $"{"position".ToQuantity(NumberOfPositions)} available";
        public string OutcomeDescription { get; internal set; }
        public string PossibleStartDate { get; internal set; }
        public string ProviderName { get; internal set; }
        public string ProviderAddress { get; internal set; }
        public IEnumerable<string> Qualifications { get; internal set; }
        public string ShortDescription { get; internal set; }
        public IEnumerable<string> Skills { get; internal set; }
        public string ThingsToConsider { get; internal set; }
        public string Title { get; internal set; }
        public string TrainingDescription { get; internal set; }
        public string TrainingTitle { get; internal set; }
        public string TrainingType { get; internal set; }
        public string TrainingLevel { get; internal set; }
        public string VacancyDescription { get; internal set; }
        public string VacancyReferenceNumber { get; internal set; }
        public string WageInfo { get; internal set; }
        public string WageText { get; internal set; }
        public string WorkingWeekDescription { get; internal set; }

        public bool HasMapUrl => !string.IsNullOrEmpty(MapUrl);

        public bool HasTrainingProviderDetails => !string.IsNullOrEmpty(ProviderName);

        public bool HasContactDetails =>    !string.IsNullOrEmpty(ContactName) 
                                            && !string.IsNullOrEmpty(ContactEmail)
                                            && !string.IsNullOrEmpty(ContactTelephone);

        public VacancyPreviewSectionState DescriptionSectionState { get; internal set; }

        public VacancyPreviewSectionState SkillsSectionState { get; internal set; }

        public VacancyPreviewSectionState QualificationsSectionState { get; internal set; }

        public IList<string> OrderedFieldNames => new List<string>
        {
            // This list is incomplete
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
