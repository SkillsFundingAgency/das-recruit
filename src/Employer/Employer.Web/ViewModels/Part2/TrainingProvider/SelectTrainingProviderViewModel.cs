using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class SelectTrainingProviderViewModel
    {
        public string Title { get; set; }
        public string Ukprn { get; set; }
        public string TrainingProviderSearch { get; set; }
        public IEnumerable<string> TrainingProviders { get; set; }
        public string FindProviderUrl { get; set; }
        public bool? SelectTrainingProvider { get; set; }
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
    }
}
