using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels.ApplicationReviews
{
    public class ApplicationReviewsFeedbackViewModel : ApplicationReviewsToUnsuccessfulRouteModel
    {
        public string ApplicationsToUnsuccessfulFeedbackHeaderTitle => IsMultipleApplications ? "Give feedback to unsuccessful applicants" : "Give feedback to the unsuccessful applicant";
        public string ApplicationsToUnsuccessfulFeedbackDescription =>
            IsMultipleApplications ? "Your feedback will be sent to all applicants you have selected as unsuccessful." : "Your feedback will be sent to the applicant you have selected as unsuccessful.";
    }
}
