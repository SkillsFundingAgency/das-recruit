using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews;

public class ApplicationReviewsToUnsuccessfulConfirmationViewModel : ApplicationReviewsToUnsuccessfulRouteModel
{
    public IList<VacancyApplication> ApplicationsToUnsuccessful { get; set; }
    public bool? ApplicationsToUnsuccessfulConfirmed { get; set; }
    public bool IsMultipleApplications
    {
        get => ApplicationsToUnsuccessful != null && ApplicationsToUnsuccessful.Count > 1;
    }
    public string ApplicationsToUnsuccessfulFeedbackHeaderTitle => IsMultipleApplications ? "Make multiple applications unsuccessful" : "Make application unsuccessful";
    public string ApplicationsToUnsuccessfulHeaderDescription =>
        IsMultipleApplications ? "You will make these applications unsuccessful:" : "You will make this application unsuccessful:";
    public string ApplicationsToUnsuccessfulNotificationMessage=>
        IsMultipleApplications ? "These applicants will be notified with this message:" : "This applicant will be notified with this message:";
    public string LegendMessage =>
        IsMultipleApplications ? "Do you want to make these applications unsuccessful?" : "Do you want to make this application unsuccessful?";
    public string CandidateFeedback { get; set; }
}