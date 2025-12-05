using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.ViewModels.ManageNotifications;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers;

[Route(RoutePaths.AccountRoutePath)]
public class ManageNotificationsController(ManageNotificationsOrchestrator orchestrator) : Controller
{
    [HttpGet("notifications-manage", Name = RouteNames.ManageNotifications_Get)]
    public async Task<IActionResult> ManageNotifications([FromRoute] long ukprn, [FromQuery] string updated)
    {
        var vm = await orchestrator.NewGetManageNotificationsViewModelAsync(User.ToVacancyUser());
        vm.Ukprn = ukprn;
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
            return RedirectToRoute(RouteNames.ManageNotifications_Get, new { updated = true, model.Ukprn });
        }

        var vm = new ManageNotificationsViewModelEx
        {
            VacancyApprovedOrRejectedValue = model.VacancyApprovedOrRejectedValue,
            ApplicationSubmittedFrequencyValue = model.ApplicationSubmittedFrequencyValue,
            ApplicationSubmittedValue = model.ApplicationSubmittedValue,
            SharedApplicationReviewedValue = model.SharedApplicationReviewedValue,
            ProviderAttachedToVacancyValue = model.ProviderAttachedToVacancyValue,
        };
        return View("NewManageNotifications", vm);
    }

    [HttpGet("notifications-unsubscribe", Name = RouteNames.ConfirmUnsubscribeNotifications_Get)]
    public IActionResult ConfirmUnsubscribeNotifications([FromRoute]long ukprn)
    {
        return View(new ConfirmUnsubscribeNotificationsViewModel(ukprn));
    }

    [HttpPost("notifications-unsubscribe", Name = RouteNames.ConfirmUnsubscribeNotifications_Post)]
    public async Task<IActionResult> ConfirmUnsubscribeNotifications(ConfirmUnsubscribeNotificationsEditModel model)
    {
        if(!ModelState.IsValid)
            return View(new ConfirmUnsubscribeNotificationsViewModel(model.Ukprn));

        if(model.ConfirmUnsubscribe == false)
            return RedirectToRoute(RouteNames.ManageNotifications_Get, new {ukprn = User.GetUkprn().ToString()});

        await orchestrator.UnsubscribeUserNotificationsAsync(User.ToVacancyUser());
            
        return RedirectToRoute(RouteNames.NotificationUnsubscribedAcknowledgement_Get, new {ukprn = User.GetUkprn().ToString()});
    }

    [HttpGet("notifications-acknowledgement", Name = RouteNames.NotificationsUpdatedAcknowledgement_Get)]
    public IActionResult NotificationsUpdatedAcknowledgement(ManageNotificationsAcknowledgementViewModel model)
    {
        return View(model);
    }

    [HttpGet("notifications-unsubscribed", Name = RouteNames.NotificationUnsubscribedAcknowledgement_Get)]
    public IActionResult NotificationUnsubscribedAcknowledgement([FromRoute]long ukprn)
    {
        return View(ukprn);
    }
}