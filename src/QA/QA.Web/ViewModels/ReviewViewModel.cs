using System.Collections.Generic;

namespace Esfa.Recruit.Qa.Web.ViewModels
{
    public class ReviewViewModel
    {
        public string Title { get; internal set; }
        public string EmployerName { get; internal set; }
        public string ShortDescription { get; internal set; }
        public string ClosingDate { get; internal set; }
        public string ApplicationInstructions { get; internal set; }
        public string ApplicationUrl { get; internal set; }
        public string ContactName { get; internal set; }
        public string ContactEmail { get; internal set; }
        public string ContactTelephone { get; internal set; }
        public string EmployerDescription { get; internal set; }
        public string EmployerWebsiteUrl { get; internal set; }
        public IEnumerable<string> EmployerAddressElements { get; internal set; }
        public string NumberOfPositionsCaption { get; internal set; }
        public string OutcomeDescription { get; internal set; }
        public string HoursPerWeek { get; internal set; }
        public string PossibleStartDate { get; internal set; }
        public string ProviderName { get; internal set; }
        public string ThingsToConsider { get; internal set; }
        public string TrainingDescription { get; internal set; }
        public string VacancyDescription { get; internal set; }
        public string VacancyReferenceNumber { get; internal set; }
        public string TrainingTitle { get; internal set; }
        public string TrainingType { get; internal set; }
        public string TrainingLevel { get; internal set; }
        public string ExpectedDuration { get; internal set; }
        public string WageInfo { get; internal set; }
        public string WorkingWeekDescription { get; internal set; }
        public string MapUrl { get; internal set; }
        public IEnumerable<string> Qualifications { get; internal set; }
        public IEnumerable<string> Skills { get; internal set; }
        public string WageText { get; internal set; }
    }
}