using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews
{
    public class ApplicationReviewsFeedbackViewModel : ApplicationReviewsToUnsuccessfulRouteModel, IApplicationReviewsEditModel
    {
        public string CandidateFeedback { get; set; }
        public List<VacancyApplication> ApplicationsToUnsuccessful { get; set; }
        public string ApplicationsToUnsuccessfulFeedbackHeaderTitle =>
            IsMultipleApplications 
                ? "Give feedback to the unsuccessful applicants" 
                : "Give feedback to the unsuccessful applicant";
        public string ApplicationsToUnsuccessfulFeedbackDescription =>
            IsMultipleApplications 
                ? "Help the applicants understand why their application was unsuccessful. Your feedback will be sent to all applicants you have selected as unsuccessful." 
                : "Help the applicant understand why their application was unsuccessful. Your message will be sent to the applicant.";
    }
}