using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.ViewModels.ManageNotifications;
using Esfa.Recruit.Shared.Web.Extensions;
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
}