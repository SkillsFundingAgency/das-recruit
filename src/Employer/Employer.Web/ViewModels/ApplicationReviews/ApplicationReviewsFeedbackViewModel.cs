using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Employer.Web.ViewModels.ApplicationReviews
{
    public class ApplicationReviewsFeedbackViewModel : ApplicationReviewsToUnsuccessfulRouteModel, IApplicationReviewsEditModel
    {
        public string CandidateFeedback { get; set; }
        public List<VacancyApplication> ApplicationsToUnsuccessful { get; set; }
    }
}