using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview
{
    public class ApplicationReviewEditModel : ApplicationReviewRouteModel
    {
        public ApplicationReviewStatus? Outcome { get; set; }
        public string CandidateFeedback { get; set; }
    }
}
