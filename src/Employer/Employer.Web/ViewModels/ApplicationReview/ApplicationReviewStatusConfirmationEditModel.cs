using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview;

namespace Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview
{
    public class ApplicationReviewStatusConfirmationEditModel : ApplicationReviewEditModel,IApplicationStatusConfirmationEditViewModel
    {
        public bool? NotifyApplicant { get; set; }
        public bool CanNotifyApplicant => NotifyApplicant.HasValue && NotifyApplicant.Value;
    }
}