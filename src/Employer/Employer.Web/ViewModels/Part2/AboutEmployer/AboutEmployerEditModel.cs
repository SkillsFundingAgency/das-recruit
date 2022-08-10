using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class AboutEmployerEditModel : VacancyRouteModel
    {
        public string EmployerDescription { get; set; }
        public string EmployerWebsiteUrl { get; set; }
        public bool IsDisabilityConfident { get; set; }
    }
}
