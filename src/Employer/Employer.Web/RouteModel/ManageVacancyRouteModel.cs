using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.RouteModel
{
    public class ManageVacancyRouteModel : VacancyRouteModel
    {
        public SortColumn? SortColumn { get; set; }
        public SortOrder? SortOrder { get; set; }
    }
}
