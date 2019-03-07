using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators.Reports;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Reports
{
    [Route(RoutePaths.ReportsRoutePath)]
    public class ReportDashboardController : Controller
    {
        private readonly ReportDashboardOrchestrator _orchestrator;

        public ReportDashboardController(ReportDashboardOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("dashboard", Name = RouteNames.ReportDashboard_Get)]
        public async Task<IActionResult> Dashboard([FromRoute] long ukprn)
        {
            var vm = await _orchestrator.GetDashboardViewModel(ukprn);
            return View(vm);
        }
    }
}