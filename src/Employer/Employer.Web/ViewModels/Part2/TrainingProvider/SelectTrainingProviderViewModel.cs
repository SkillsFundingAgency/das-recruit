using Esfa.Recruit.Employer.Web.Views;
using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class SelectTrainingProviderViewModel
    {
        public string Title { get; set; }
        [Display(Name = "UKPRN")]
        public string Ukprn { get; set; }
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
    }
}
