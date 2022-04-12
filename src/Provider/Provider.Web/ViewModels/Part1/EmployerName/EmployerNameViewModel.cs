using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.EmployerName
{
    public class EmployerNameViewModel : VacancyRouteModel
    {
        public string LegalEntityName { get; set; }
        public string ExistingTradingName { get; set; } 
        public string NewTradingName { get; set; }
        public string AnonymousName { get; set; }
        public string AnonymousReason { get; set; }
        public EmployerIdentityOption? SelectedEmployerIdentityOption { get; set; }

        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public PartOnePageInfoViewModel PageInfo { get; set; }
        public bool HasExistingTradingName => string.IsNullOrWhiteSpace(ExistingTradingName) == false;

        public bool HasOnlyOneOrganisation { get; internal set; }
        public string Title { get; set; }
        public bool IsTaskListCompleted { get; set; }
    }
}