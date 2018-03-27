using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class ApplicationProcessEditModel : VacancyRouteModel
    {
        public string ApplicationInstructions { get; set; }
        public string ApplicationUrl { get; set; }
        public string EmployerContactName { get; set; }
        public string EmployerContactEmail { get; set; }
        public string EmployerContactPhone { get; set; }
    }
}
