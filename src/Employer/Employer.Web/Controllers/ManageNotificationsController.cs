using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.ManageNotifications;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountRoutePath)]
    public class ManageNotificationsController : Controller
    {
        private readonly ManageNotificationsOrchestrator _orchestrator;
        public ManageNotificationsController(ManageNotificationsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("notifications-manage", Name = RouteNames.ManageNotifications_Get)]
        public async Task<IActionResult> ManageNotifications()
        {
            var vm = await _orchestrator.GetManageNotificationsViewModelAsync(User.ToVacancyUser());
            return View(vm);
        }

        [HttpPost("notifications-manage", Name = RouteNames.ManageNotifications_Post)]
        public async Task<IActionResult> ManageNotifications(ManageNotificationsEditModel model)
        {
            await _orchestrator.UpdateUserNotificationPreferencesAsync(model, User.ToVacancyUser());

            if(model.HasAnySubscription)
            {
                var vm = _orchestrator.GetAcknowledgementViewModel(model, User.ToVacancyUser());
                return View(ViewNames.NotificationsUpdatedAcknowledgement, vm);
            }

            return View(ViewNames.NotificationUnsubscribedAcknowledgement);
        }

        [HttpGet("notifications-unsubscribe", Name = RouteNames.ConfirmUnsubscribeNotifications_Get)]
        public IActionResult ConfirmUnsubscribeNotifications()
        {
            return View(new ConfirmUnsubscribeNotificationsViewModel());
        }

        [HttpPost("notifications-unsubscribe", Name = RouteNames.ConfirmUnsubscribeNotifications_Post)]
        public async Task<IActionResult> ConfirmUnsubscribeNotifications(ConfirmUnsubscribeNotificationsEditModel model)
        {
            if(!ModelState.IsValid)
                return View(new ConfirmUnsubscribeNotificationsViewModel());

            if(model.ConfirmUnsubscribe == false)
                return RedirectToRoute(RouteNames.ManageNotifications_Get);

            await _orchestrator.UnsubscribeUserNotificationsAsync(User.ToVacancyUser());
            
            return View(ViewNames.NotificationUnsubscribedAcknowledgement);
        }
    }
}