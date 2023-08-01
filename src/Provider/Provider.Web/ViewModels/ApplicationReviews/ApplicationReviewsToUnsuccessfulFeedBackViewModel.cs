using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews
{
    public class ApplicationReviewsToUnsuccessfulFeedbackViewModel : ApplicationReviewsToUnsuccessfulModel
    {
        public string ApplicationsToUnsuccessfulFeedbackHeaderTitle => IsMultipleApplications ? "Give feedback to unsuccessful applicant's" : "Give feedback to unsuccessful applicant";
        public string ApplicationsToUnsuccessfulFeedbackDescription =>
            IsMultipleApplications ? "Your feedback will be sent to all applicants you have selected as unsuccessful." : "Your feedback will be sent to the applicant you have selected as unsuccessful.";

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(Outcome)
        };
    }
}
