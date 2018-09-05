using System;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Qa.Web.ViewModels
{
    public class ReviewEditModel
    {
        [FromRoute]
        public Guid ReviewId {get;set;}

        public bool EmployerNameChecked { get; set; }
        public bool ShortDescriptionChecked { get; set; }
        public bool ClosingDateChecked { get; set; }
        public bool WorkingWeekChecked { get; set; }
        public bool WageChecked { get; set; }
        public bool ExpectedDurationChecked { get; set; }
        public bool PossibleStartDateChecked { get; set; }
        public bool TrainingLevelChecked { get; set; }
        public bool NumberOfPositionsChecked { get; set; }
        public bool VacancyDescriptionChecked { get; set; }
        public bool TrainingDescriptionChecked { get; set; }
        public bool OutcomeDescriptionChecked { get; set; }
        public bool SkillsChecked { get; set; }
        public bool QualificationsChecked { get; set; }
        public bool ThingsToConsiderChecked { get; set; }
        public bool EmployerDescriptionChecked { get; set; }
        public bool EmployerWebsiteUrlChecked { get; set; }
        public bool ContactChecked { get; set; }
        public bool EmployerAddressChecked { get; set; }
        public bool ProviderChecked { get; set; }
        public bool TrainingChecked { get; set; }
        public bool ApplicationProcessChecked { get; set; }
        public string ReviewerComment { get; set; }
    }
}
