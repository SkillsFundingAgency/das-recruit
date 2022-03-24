using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part2.Considerations
{
    public class ConsiderationsViewModel : VacancyRouteModel
    {
        public string Title { get; internal set; }
        public string ThingsToConsider { get; internal set; }
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
    }
}
