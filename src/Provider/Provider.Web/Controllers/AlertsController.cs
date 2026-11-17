using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers;

[Route(RoutePaths.AccountRoutePath)]
public class AlertsController(AlertsOrchestrator orchestrator) : Controller
{
    [HttpPost("dismiss-alert", Name = RouteNames.Alerts_Dismiss_Post)]
    public async Task<IActionResult> DismissAlert(AlertDismissalEditModel model)
    {
        if (Enum.TryParse<AlertType>(model.AlertType, out var alertTypeEnum))
        {
            await orchestrator.DismissAlert(User.ToVacancyUser(), alertTypeEnum);
        }

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToRoute(RouteNames.Dashboard_Get, new {model.Ukprn});
    }
}