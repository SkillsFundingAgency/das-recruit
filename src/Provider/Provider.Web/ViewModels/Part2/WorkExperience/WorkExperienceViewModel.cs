using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part2.WorkExperience
{
    public class WorkExperienceViewModel : VacancyRouteModel
    {
        public string Title { get; internal set; }
        public string WorkExperience { get; internal set; }
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
        public bool IsTaskListCompleted { get; set; }
    }
}