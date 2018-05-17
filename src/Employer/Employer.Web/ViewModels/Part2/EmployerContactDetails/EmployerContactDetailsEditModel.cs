using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class EmployerContactDetailsEditModel : VacancyRouteModel
    {
        public string EmployerContactName { get; set; }
        public string EmployerContactEmail { get; set; }
        public string EmployerContactPhone { get; set; }
    }
}
