using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators.Reports;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Reports
{
    [Route(RoutePaths.ReportRoutePath)]
    public class ReportConfirmationController : Controller
    {
        private readonly ReportConfirmationOrchestrator _orchestrator;
        public ReportConfirmationController(ReportConfirmationOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("confirmation", Name = RouteNames.ReportConfirmation_Get)]
        public async Task<IActionResult> Confirmation(ReportRouteModel rrm)
        {
            var vm = await _orchestrator.GetConfirmationViewModelAsync(rrm);
            return View(vm);
        }
    }
}
