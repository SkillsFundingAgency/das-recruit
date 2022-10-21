using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.ManageNotifications;
using Esfa.Recruit.Shared.Web.Extensions;
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
        public async Task<IActionResult> ManageNotifications([FromRoute] string employerAccountId)
        {
            var vm = await _orchestrator.GetManageNotificationsViewModelAsync(User.ToVacancyUser(), employerAccountId);
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
                var vm = await _orchestrator.GetManageNotificationsViewModelAsync(User.ToVacancyUser(), model.EmployerAccountId);
                return View(vm);
            }

            if(model.HasAnySubscription)
            {
                var vm = _orchestrator.GetAcknowledgementViewModel(model, User.ToVacancyUser());
                return RedirectToRoute(RouteNames.NotificationsUpdatedAcknowledgement_Get, vm);
            }

            return RedirectToRoute(RouteNames.NotificationUnsubscribedAcknowledgement_Get, new {model.EmployerAccountId});
        }

        [HttpGet("notifications-unsubscribe", Name = RouteNames.ConfirmUnsubscribeNotifications_Get)]
        public IActionResult ConfirmUnsubscribeNotifications(ManageNotificationsRouteModel model)
        {
            return View(new ConfirmUnsubscribeNotificationsViewModel{EmployerAccountId = model.EmployerAccountId});
        }

        [HttpPost("notifications-unsubscribe", Name = RouteNames.ConfirmUnsubscribeNotifications_Post)]
        public async Task<IActionResult> ConfirmUnsubscribeNotifications(ConfirmUnsubscribeNotificationsEditModel model)
        {
            if(!ModelState.IsValid)
                return View(new ConfirmUnsubscribeNotificationsViewModel{EmployerAccountId = model.EmployerAccountId});

            if(model.ConfirmUnsubscribe == false)
                return RedirectToRoute(RouteNames.ManageNotifications_Get, new {model.EmployerAccountId});

            await _orchestrator.UnsubscribeUserNotificationsAsync(User.ToVacancyUser());
            
            return RedirectToRoute(RouteNames.NotificationUnsubscribedAcknowledgement_Get, new {model.EmployerAccountId});
        }

        [HttpGet("notifications-acknowledgement", Name = RouteNames.NotificationsUpdatedAcknowledgement_Get)]
        public IActionResult NotificationsUpdatedAcknowledgement(ManageNotificationsAcknowledgementViewModel model)
        {
            return View(model);
        }

        [HttpGet("notifications-unsubscribed", Name = RouteNames.NotificationUnsubscribedAcknowledgement_Get)]
        public IActionResult NotificationUnsubscribedAcknowledgement(ManageNotificationsRouteModel routeModel)
        {
            return View(routeModel);
        }
    }
}