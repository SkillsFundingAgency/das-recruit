using System.ComponentModel.DataAnnotations;
using ValMsg = Esfa.Recruit.Shared.Web.ViewModels.ValidationMessages;

namespace Esfa.Recruit.Provider.Web.ViewModels.ManageNotifications
{
    public class ConfirmUnsubscribeNotificationsEditModel
    {
        [Required(ErrorMessage = ValMsg.UnsubscribeNotificationsConfirmationMessages.SelectionRequired)]
        public bool? ConfirmUnsubscribe { get; set; }
    }
}