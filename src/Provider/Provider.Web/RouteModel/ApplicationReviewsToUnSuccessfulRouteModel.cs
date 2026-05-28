using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.RouteModel
{
    public class ApplicationReviewsToUnsuccessfulRouteModel : VacancyRouteModel
    {
        public ApplicationReviewStatus? Outcome { get; set; }
        public bool IsMultipleApplications { get; set; }
    }
}