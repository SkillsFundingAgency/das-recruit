using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Configuration.Routing;
using Esfa.Recruit.Qa.Web.Orchestrators;
using Esfa.Recruit.Qa.Web.ViewModels;
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
        public async Task<IActionResult> Index()
        {
            var vm = await _orchestrator.GetDashboardViewModelAsync();
            
            return View(vm);
        }

        [HttpPost("/", Name = RouteNames.Dashboard_Index_Post)]
        public async Task<IActionResult> Index(string searchTerm)
        {
            var vm = await _orchestrator.GetDashboardViewModelAsync().ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                vm.SearchResults = await _orchestrator.GetSearchResultsAsync(searchTerm).ConfigureAwait(true);
                vm.LastSearchTerm = searchTerm;
                vm.IsPostBack = true;
            }
            return View(vm);
        }
    }
}
