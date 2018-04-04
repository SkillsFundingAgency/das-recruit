using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class PreviewController : Controller
    {
        private readonly PreviewOrchestrator _orchestrator;

        public PreviewController(PreviewOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }
        
        [HttpGet("search-result-preview", Name = RouteNames.SearchResultPreview_Get)]
        public async Task<IActionResult> Preview(Guid vacancyId)
        {
            var vm = await _orchestrator.GetPreviewViewModelAsync(vacancyId);
            return View(vm);
        }
    }
}