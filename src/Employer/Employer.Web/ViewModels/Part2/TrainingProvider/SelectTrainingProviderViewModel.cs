using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class SelectTrainingProviderViewModel
    {
        public string Title { get; set; }
        [Display(Name = "UKPRN")]
        public string Ukprn { get; set; }
        public string TrainingProviderSearch { get; set; }
        public string SelectedTrainingText { get; set; }
        public IEnumerable<string> TrainingProviders { get; set; }
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
    }
}
