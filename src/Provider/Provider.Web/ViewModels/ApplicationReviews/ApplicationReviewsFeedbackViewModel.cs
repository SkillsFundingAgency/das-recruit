using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews
{
    public class ApplicationReviewsFeedbackViewModel : ApplicationReviewsToUnsuccessfulRouteModel, IApplicationReviewsEditModel
    {
        public required string CandidateFeedback { get; set; }
        public List<VacancyApplication> ApplicationsToUnsuccessful { get; set; }
    }
}