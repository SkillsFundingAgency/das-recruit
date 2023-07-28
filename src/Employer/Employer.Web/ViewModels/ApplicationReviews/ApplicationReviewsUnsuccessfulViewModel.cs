using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Employer.Web.ViewModels.ApplicationReviews
{
    public class ApplicationReviewsUnsuccessfulViewModel : ApplicationReviewsToUnsuccessfulRouteModel
    {
        public long VacancyReference { get; set; }
        public List<VacancyApplication> VacancyApplications { get; set; }
    }
}
