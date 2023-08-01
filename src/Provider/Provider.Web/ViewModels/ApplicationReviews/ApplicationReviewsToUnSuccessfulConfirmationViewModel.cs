using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews
{
    public class ApplicationReviewsToUnSuccessfulConfirmationViewModel : ApplicationReviewsStatusChangeModel
    {
        public new IList<VacancyApplication> ApplicationsToUnSuccessful { get; set; }
        public bool? ApplicationsToUnSuccessfulConfirmed { get; set; }
        public override bool IsMultipleApplications
        {
            get => ApplicationsToUnSuccessful != null && ApplicationsToUnSuccessful.Count > 1;
        }
        public string ApplicationsToUnSuccessfulFeedbackHeaderTitle => IsMultipleApplications ? "Make multiple applications unsuccessful" : "Make application unsuccessful";
        public string ApplicationsToUnSuccessfulHeaderDescription =>
            IsMultipleApplications ? "You will make these applications unsuccessful:" : "You will make this application unsuccessful:";
        public string ApplicationsToUnSuccessfulNotificationMessage=>
            IsMultipleApplications ? "These applicants will be notified with this message:" : "This applicant will be notified with this message:";
        public string LegendMessage =>
            IsMultipleApplications ? "Do you want to make these applications unsuccessful?" : "Do you want to make this application unsuccessful?";
    }
}
