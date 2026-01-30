using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.ManageNotifications;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers;

[Route(RoutePaths.AccountRoutePath)]
public class ManageNotificationsController(ManageNotificationsOrchestrator orchestrator) : Controller
{
    [HttpGet("notifications-manage", Name = RouteNames.ManageNotifications_Get)]
    public async Task<IActionResult> ManageNotifications([FromRoute] string employerAccountId, [FromQuery] string updated)
    {
        var vm = await orchestrator.NewGetManageNotificationsViewModelAsync(User.ToVacancyUser(), employerAccountId);
        if (bool.TryParse(updated, out bool updatedValue))
        {
            vm.Updated = updatedValue;
        }
        return View("NewManageNotifications", vm);
    }
        
    [HttpPost("notifications-manage", Name = RouteNames.ManageNotifications_Post)]
    public async Task<IActionResult> ManageNotifications(ManageNotificationsEditModelEx model)
    {
        var response = await orchestrator.NewUpdateUserNotificationPreferencesAsync(model, User.ToVacancyUser());
        if (!response.Success)
        {
            response.AddErrorsToModelState(ModelState);
        }

        if (ModelState.IsValid)
        {
            return RedirectToRoute(RouteNames.ManageNotifications_Get, new { updated = true, model.EmployerAccountId });
        }

        var vm = new ManageNotificationsViewModelEx
        {
            VacancyApprovedOrRejectedValue = model.VacancyApprovedOrRejectedValue,
            ApplicationSubmittedFrequencyValue = model.ApplicationSubmittedFrequencyValue,
            ApplicationSubmittedValue = model.ApplicationSubmittedValue,
        };
        return View("NewManageNotifications", vm);
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

        await orchestrator.UnsubscribeUserNotificationsAsync(User.ToVacancyUser());
            
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