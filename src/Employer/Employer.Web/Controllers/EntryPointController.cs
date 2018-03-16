using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePrefixPaths.AccountRoutePath)]
    public class EntryPointController : Controller
    {
        private readonly ILogger<EntryPointController> _logger;
        private readonly EntryPointOrchestrator _orchestrator;

        public EntryPointController(ILogger<EntryPointController> logger, EntryPointOrchestrator orchestrator)
        {
            _logger = logger;
            _orchestrator = orchestrator;
        }

        [HttpGet("")]
        public async Task<IActionResult> EntryPoint(string employerAccountId)
        {
            await _orchestrator.RecordUserSignInAsync(employerAccountId);

            return RedirectToRoute(RouteNames.Dashboard_Index_Get);
        }
    }
}