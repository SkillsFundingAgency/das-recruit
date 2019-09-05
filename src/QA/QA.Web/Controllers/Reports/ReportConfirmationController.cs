using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Configuration.Routing;
using Esfa.Recruit.Qa.Web.Orchestrators.Reports;
using Esfa.Recruit.Qa.Web.RouteModel;
using Esfa.Recruit.Qa.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Qa.Web.Controllers.Reports
{
    [Authorize(Policy = AuthorizationPolicyNames.TeamLeadUserPolicyName)]
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
