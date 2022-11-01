using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class ConsiderationsViewModel : VacancyRouteModel
    {
        public string Title { get; internal set; }
        public string ThingsToConsider { get; internal set; }
        public ReviewSummaryViewModel Review { get; set; } = new();
        public bool IsTaskListCompleted { get ; set ; }
    }
}
