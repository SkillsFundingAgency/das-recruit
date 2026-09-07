using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;

public class ApplicationReviewStatusChangeModel : ApplicationReviewRouteModel, IApplicationReviewEditModel
{
    public ApplicationReviewStatus? Outcome { get; set; }
    public string CandidateFeedback { get; set; }
    public bool IsApplicationSharedByProvider { get; set; }
}