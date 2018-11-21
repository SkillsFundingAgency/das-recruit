using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePrefixPaths.AccountRoutePath)]
    public class DashboardController : Controller
    {
        private readonly DashboardOrchestrator _orchestrator;

        public DashboardController(DashboardOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("", Name = RouteNames.Dashboard_Index_Get)]
        public IActionResult Dashboard([FromRoute]string ukprn)
        {
            //var vm = await _orchestrator.GetDashboardViewModelAsync(ukprn);

            return View(new DashboardViewModel());
        }
    }
}