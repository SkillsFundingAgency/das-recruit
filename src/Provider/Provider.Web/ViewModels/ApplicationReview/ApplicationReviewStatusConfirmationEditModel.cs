using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReview
{
    public class ApplicationReviewStatusConfirmationEditModel : ApplicationReviewStatusChangeModel, IApplicationStatusConfirmationEditViewModel
    {
        public bool? NotifyCandidate { get; set; }
        public bool CanNotifyCandidate => NotifyCandidate.HasValue && NotifyCandidate.Value;    
    }
}