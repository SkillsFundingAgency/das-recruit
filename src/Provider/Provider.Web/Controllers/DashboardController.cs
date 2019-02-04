using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
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

        [HttpGet("", Name = RouteNames.Dashboard_Index_Get)]
        public async Task<IActionResult> Dashboard()
        {
            var vm = await _orchestrator.GetDashboardViewModelAsync(User.GetUkprn());

            return View(vm);
        }

        [HttpGet(RoutePaths.VacancyRoutePath, Name = RouteNames.DisplayVacancy_Get)]
        public async Task<IActionResult> DisplayVacancy(VacancyRouteModel vrm)
        {
            var route = await _orchestrator.GetVacancyRedirectRouteAsync(vrm);
            return RedirectToRoute(route);
        }
    }
}