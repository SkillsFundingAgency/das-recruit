using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews
{
    public class ApplicationReviewsToUnsuccessfulModel : ApplicationReviewsToUnsuccessfulRouteModel, IApplicationReviewEditModel
    {
        public ApplicationReviewStatus? Outcome { get; set; }
        public string CandidateFeedback { get; set; }
        public bool NavigateToFeedbackPage { get; set; }
        public virtual bool IsMultipleApplications
        {
            get => ApplicationsToUnsuccessful != null && ApplicationsToUnsuccessful.Count > 1;
        }
        public NavigationTarget TargetView { get; set; }
    }
}
