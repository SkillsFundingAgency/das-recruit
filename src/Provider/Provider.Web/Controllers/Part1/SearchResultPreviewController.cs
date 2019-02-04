using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;

namespace  Esfa.Recruit.Provider.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class SearchResultPreviewController : Controller
    {
        private readonly SearchResultPreviewOrchestrator _orchestrator;

        public SearchResultPreviewController(SearchResultPreviewOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }
        
        [HttpGet("search-result-preview", Name = RouteNames.SearchResultPreview_Get)]
        public async Task<IActionResult> SearchResultPreview(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetSearchResultPreviewViewModelAsync(vrm);
            return View(vm);
        }
    }
}