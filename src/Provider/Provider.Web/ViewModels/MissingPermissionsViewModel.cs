using Esfa.Recruit.Provider.Web.Model;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Models;

namespace Esfa.Recruit.Provider.Web.ViewModels
{
    public class MissingPermissionsViewModel
    {
        public VacancyRouteModel RouteValues { get; internal set; }
        public VacancyActionType ActionType { get; internal set; }
        public string CtaRoute { get; internal set; }
    }
}