using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews;

public class ApplicationReviewsToUnsuccessfulConfirmationViewModel : ApplicationReviewsToUnsuccessfulRouteModel
{
    public IList<VacancyApplication> ApplicationsToUnsuccessful { get; set; }
    public bool? ApplicationsToUnsuccessfulConfirmed { get; set; }
    public string ApplicationsToUnsuccessfulFeedbackHeaderTitle => 
        IsMultipleApplications 
            ? "Do you want to make these applications unsuccessful?" 
            : "Do you want to make this application unsuccessful?";
    public string CandidateFeedback { get; set; }
}