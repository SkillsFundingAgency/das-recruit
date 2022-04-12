using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.ViewModels.ManageNotifications;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
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
            var response = await _orchestrator.UpdateUserNotificationPreferencesAsync(model, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if(!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetManageNotificationsViewModelAsync(User.ToVacancyUser());
                return View(vm);
            }

            if(model.HasAnySubscription)
            {
                var vm = _orchestrator.GetAcknowledgementViewModel(model, User.ToVacancyUser());
                return RedirectToRoute(RouteNames.NotificationsUpdatedAcknowledgement_Get, vm);
            }

            return RedirectToRoute(RouteNames.NotificationUnsubscribedAcknowledgement_Get);
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
            
            return RedirectToRoute(RouteNames.NotificationUnsubscribedAcknowledgement_Get);
        }

        [HttpGet("notifications-acknowledgement", Name = RouteNames.NotificationsUpdatedAcknowledgement_Get)]
        public IActionResult NotificationsUpdatedAcknowledgement(ManageNotificationsAcknowledgementViewModel model)
        {
            return View(model);
        }

        [HttpGet("notifications-unsubscribed", Name = RouteNames.NotificationUnsubscribedAcknowledgement_Get)]
        public IActionResult NotificationUnsubscribedAcknowledgement()
        {
            return View();
        }
    }
}