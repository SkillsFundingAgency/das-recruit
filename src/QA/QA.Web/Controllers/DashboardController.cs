using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Configuration.Routing;
using Esfa.Recruit.Qa.Web.Orchestrators;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Qa.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly DashboardOrchestrator _orchestrator;

        public DashboardController(DashboardOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("/", Name = RouteNames.Dashboard_Index_Get)]
        public async Task<IActionResult> Index([FromQuery]string searchTerm)
        {
            var vm = await _orchestrator.GetDashboardViewModelAsync();

            if (string.IsNullOrWhiteSpace(searchTerm)) return View(vm);

            vm.SearchResults = await _orchestrator.GetSearchResultsAsync(searchTerm);
            vm.LastSearchTerm = searchTerm;
            vm.IsPostBack = true;
            return View(vm);
        }
    }
}
