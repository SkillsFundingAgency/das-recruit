using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Employer.Web.ViewModels.ApplicationReviews
{
    public class ApplicationReviewsFeedbackViewModel : ApplicationReviewsToUnsuccessfulRouteModel,IApplicationReviewsEditModel
    {
        public override bool IsMultipleApplications => ApplicationsToUnsuccessful.Count > 1;
        public string CandidateFeedback { get; set; }
        public List<VacancyApplication> ApplicationsToUnsuccessful { get; set; }
        public string ApplicationsToUnsuccessfulFeedbackHeaderTitle => IsMultipleApplications ? "Give feedback to the unsuccessful applicants" : "Give feedback to the unsuccessful applicant";
        public string ApplicationsToUnsuccessfulFeedbackDescription =>
            IsMultipleApplications ? "Your feedback will be sent to all applicants you have selected as unsuccessful." : "Your feedback will be sent to the applicant you have selected as unsuccessful.";
    }
}
