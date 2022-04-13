using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using ValMsg = Esfa.Recruit.Shared.Web.ViewModels.ValidationMessages;

namespace Esfa.Recruit.Provider.Web.ViewModels.ManageNotifications
{
    public class ConfirmUnsubscribeNotificationsEditModel
    {
        [FromRoute]
        public long Ukprn { get; set; }
        
        [Required(ErrorMessage = ValMsg.UnsubscribeNotificationsConfirmationMessages.SelectionRequired)]
        public bool? ConfirmUnsubscribe { get; set; }
    }
}