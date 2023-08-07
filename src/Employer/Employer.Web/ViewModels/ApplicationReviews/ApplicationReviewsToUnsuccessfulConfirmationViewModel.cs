using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Employer.Web.ViewModels.ApplicationReviews
{
    public class ApplicationReviewsToUnsuccessfulConfirmationViewModel : ApplicationReviewsToUnsuccessfulRouteModel
    {
        public IList<VacancyApplication> VacancyApplicationsToUnsuccessful { get; set; }
        public bool? ApplicationsUnsuccessfulConfirmed { get; set; }
        public override bool IsMultipleApplications
        {
            get => VacancyApplicationsToUnsuccessful != null && VacancyApplicationsToUnsuccessful.Count > 1;
        }
        public string ApplicationReviewsConfirmationHeaderTitle => IsMultipleApplications ? "Make multiple applications unsuccessful" : "Make application unsuccessful";
        public string ApplicationReviewsConfirmationHeaderDescription => IsMultipleApplications ? "You will make these applications unsuccessful:" : "You will make this application unsuccessful:";
        public string ApplicationsReviewsConfirmationNotificationMessage => IsMultipleApplications ? "These applicants will be notified with this message:" : "This applicant will be notified with this message:";
        public string ApplicationsReviewsConfirmationLegendMessage => IsMultipleApplications ? "Do you want to make these applications unsuccessful?" : "Do you want to make this application unsuccessful?";
    }
}
