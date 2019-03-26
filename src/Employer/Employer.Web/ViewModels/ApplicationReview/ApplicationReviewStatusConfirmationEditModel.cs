using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview
{
    public class ApplicationReviewStatusConfirmationEditModel : ApplicationReviewRouteModel, IApplicationStatusConfirmationEditViewModel
    {
        public bool? NotifyCandidate { get; set; }
        public bool CanNotifyCandidate => NotifyCandidate.HasValue && NotifyCandidate.Value;
        public ApplicationReviewStatus? Outcome { get; set; }
        public string CandidateFeedback { get; set; }        
    }
}