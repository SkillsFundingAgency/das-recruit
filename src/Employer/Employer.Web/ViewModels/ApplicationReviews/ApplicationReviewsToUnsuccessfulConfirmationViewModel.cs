using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Employer.Web.ViewModels.ApplicationReviews
{
    public class ApplicationReviewsToUnsuccessfulConfirmationViewModel : ApplicationReviewsToUnsuccessfulRouteModel
    {
        public IList<VacancyApplication> VacancyApplicationsToUnsuccessful { get; set; }
    }
}
