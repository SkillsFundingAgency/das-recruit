using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.EmployerName
{
    public class EmployerNameEditModel : VacancyRouteModel
    {
        public EmployerIdentityOption? SelectedEmployerIdentityOption { get; set; }
        
        public string NewTradingName { get; set; }

        public string AnonymousName { get; set; }
        public string AnonymousReason { get; set; }
    }
}