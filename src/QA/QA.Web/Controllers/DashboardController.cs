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
            var vm = await _orchestrator.GetDashboardViewModelAsync().ConfigureAwait(false);
            
            return View(vm);
        }

        /// <summary>
        /// Opted to use POST instead of GET as it keeps the URL clean of query string and path
        /// </summary>
        [HttpPost("/", Name = RouteNames.Dashboard_Index_Post)]
        public async Task<IActionResult> Index(string searchTerm)
        {
            var vm = await _orchestrator.GetDashboardViewModelAsync().ConfigureAwait(false);

            //I would prefer not to go forward if search term is empty
            //however the story says it should show no results
            //As a safe guard, in the client, I am avoiding the DB call and returning an empty collection
            vm.SearchResults = await _orchestrator.GetSearchResultsAsync(searchTerm).ConfigureAwait(true);
            vm.LastSearchTerm = searchTerm;
            vm.IsPostBack = true;
            return View(vm);
        }
    }
}
