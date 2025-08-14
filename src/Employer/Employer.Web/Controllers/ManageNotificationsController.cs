using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.ManageNotifications;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers;

[Route(RoutePaths.AccountRoutePath)]
public class ManageNotificationsController(ManageNotificationsOrchestrator orchestrator, IFeature feature) : Controller
{
    [HttpGet("notifications-manage", Name = RouteNames.ManageNotifications_Get)]
    public async Task<IActionResult> ManageNotifications([FromRoute] string employerAccountId, [FromQuery] string updated)
    {
        if (feature.IsFeatureEnabled(FeatureNames.NotificationsMigration))
        {
            var vm = await orchestrator.NewGetManageNotificationsViewModelAsync(User.ToVacancyUser(), employerAccountId);
            if (bool.TryParse(updated, out bool updatedValue))
            {
                vm.Updated = updatedValue;
            }
            return View("NewManageNotifications", vm);
        }
        else
        {
            var vm = await orchestrator.GetManageNotificationsViewModelAsync(User.ToVacancyUser(), employerAccountId);
            return View(vm);
        }
    }
        
    [HttpPost("notifications-manage", Name = RouteNames.ManageNotifications_Post)]
    public async Task<IActionResult> ManageNotifications(ManageNotificationsEditModelEx model)
    {
        if (feature.IsFeatureEnabled(FeatureNames.NotificationsMigration))
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
                VacancyApprovedOrRejectedOptionValue = model.VacancyApprovedOrRejectedOptionValue,
                ApplicationSubmittedFrequencyAllOptionValue = model.ApplicationSubmittedFrequencyAllOptionValue,
                ApplicationSubmittedFrequencyMineOptionValue = model.ApplicationSubmittedFrequencyMineOptionValue,
                ApplicationSubmittedOptionValue = model.ApplicationSubmittedOptionValue,
            };
            return View("NewManageNotifications", vm);
        }
        else
        {
            var response = await orchestrator.UpdateUserNotificationPreferencesAsync(model, User.ToVacancyUser());
            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if(!ModelState.IsValid)
            {
                var vm = await orchestrator.GetManageNotificationsViewModelAsync(User.ToVacancyUser(), model.EmployerAccountId);
                return View(vm);
            }

            if(model.HasAnySubscription)
            {
                var vm = orchestrator.GetAcknowledgementViewModel(model, User.ToVacancyUser());
                return RedirectToRoute(RouteNames.NotificationsUpdatedAcknowledgement_Get, vm);
            }

            return RedirectToRoute(RouteNames.NotificationUnsubscribedAcknowledgement_Get, new {model.EmployerAccountId});
        }
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