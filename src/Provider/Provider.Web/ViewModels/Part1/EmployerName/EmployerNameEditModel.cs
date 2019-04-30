using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Models;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.EmployerName
{
    public class EmployerNameEditModel : VacancyRouteModel
    {
        public EmployerIdentityOption? SelectedEmployerIdentityOption { get; set; }
        
        public string NewTradingName { get; set; }

        public string AnonymousName { get; set; }
        public string AnonymousReason { get; set; }
    }
}