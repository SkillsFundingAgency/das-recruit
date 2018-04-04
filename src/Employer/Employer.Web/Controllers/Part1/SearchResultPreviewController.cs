using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class SearchResultPreviewController : Controller
    {
        private readonly SearchResultPreviewOrchestrator _orchestrator;

        public SearchResultPreviewController(SearchResultPreviewOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }
        
        [HttpGet("search-result-preview", Name = RouteNames.SearchResultPreview_Get)]
        public async Task<IActionResult> SearchResultPreview(Guid vacancyId)
        {
            var vm = await _orchestrator.GetSearchResultPreviewViewModelAsync(vacancyId);
            return View(vm);
        }
    }
}