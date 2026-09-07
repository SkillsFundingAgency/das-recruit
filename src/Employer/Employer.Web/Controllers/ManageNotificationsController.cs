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
}