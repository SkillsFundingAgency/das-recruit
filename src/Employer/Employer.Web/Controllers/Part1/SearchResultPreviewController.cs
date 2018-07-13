using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
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
        public async Task<IActionResult> SearchResultPreview(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetSearchResultPreviewViewModelAsync(vrm);
            return View(vm);
        }

        [HttpPost("search-result-preview", Name = RouteNames.SearchResultPreview_Post)]
        public async Task<IActionResult> PostSearchResultPreview(VacancyRouteModel vrm)
        {
            var redirectRouteParameters = await _orchestrator.PostSearchResultPreviewViewModelAsync(vrm, User.ToVacancyUser());

            return RedirectToRoute(redirectRouteParameters.RouteName, redirectRouteParameters.RouteValues, redirectRouteParameters.Fragment);
        }
    }
}