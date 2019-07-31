using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountRoutePath)]
    public class DashboardController : Controller
    {
        private readonly DashboardOrchestrator _orchestrator;

        public DashboardController(DashboardOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("", Name = RouteNames.Dashboard_Get)]
        public async Task<IActionResult> Dashboard()
        {
            var vm = await _orchestrator.GetDashboardViewModelAsync(User.ToVacancyUser());
            return View(vm);
        }

        [HttpPost("dismiss-alert", Name = RouteNames.Dashboard_DismissAlert_Post)]
        public async Task<IActionResult> DismissAlert([FromRoute] string employerAccountId, AlertDismissalEditModel model)
        {
            if (Enum.TryParse(typeof(AlertType), model.AlertType, out var alertTypeEnum))
            {
                await _orchestrator.DismissAlert(User.ToVacancyUser(), (AlertType)alertTypeEnum);
            }

            return RedirectToRoute(RouteNames.Dashboard_Get);
        }
    }
}