using System.ComponentModel.DataAnnotations;
using ValMsg = Esfa.Recruit.Shared.Web.ViewModels.ValidationMessages;

namespace Esfa.Recruit.Employer.Web.ViewModels.ManageNotifications
{
    public class ConfirmUnsubscribeNotificationsEditModel : ManageNotificationsRouteModel
    {
        [Required(ErrorMessage = ValMsg.UnsubscribeNotificationsConfirmationMessages.SelectionRequired)]
        public bool? ConfirmUnsubscribe { get; set; }
    }
}