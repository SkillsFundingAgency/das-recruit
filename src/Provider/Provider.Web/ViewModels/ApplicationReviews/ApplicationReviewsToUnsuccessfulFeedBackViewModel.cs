using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReviews;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews;

public class ApplicationReviewsToUnsuccessfulFeedbackViewModel : VacancyRouteModel, IApplicationReviewsEditModel
{
    public bool IsMultipleApplications { get; set; }
    public string CandidateFeedback { get; set; }
    public Dictionary<string, string> ApplicationDetails = new Dictionary<string, string>();
}