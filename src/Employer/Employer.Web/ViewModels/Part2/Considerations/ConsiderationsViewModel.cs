using Esfa.Recruit.Employer.Web.Views;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class ConsiderationsViewModel
    {
        public string Title { get; internal set; }
        public string ThingsToConsider { get; internal set; }
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
    }
}
