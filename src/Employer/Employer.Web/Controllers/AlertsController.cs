using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountRoutePath)]
    public class AlertsController : Controller
    {
        private readonly AlertsOrchestrator _orchestrator;

        public AlertsController(AlertsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpPost("dismiss-alert", Name = RouteNames.Alerts_Dismiss_Post)]
        public async Task<IActionResult> DismissAlert(AlertDismissalEditModel model)
        {
            if (Enum.TryParse<AlertType>(model.AlertType, out var alertTypeEnum))
            {
                await _orchestrator.DismissAlert(User.ToVacancyUser(), alertTypeEnum);
            }

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToRoute(RouteNames.Dashboard_Get);
        }
    }
}