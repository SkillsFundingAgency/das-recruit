using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Configuration.Routing;
using Esfa.Recruit.Qa.Web.Orchestrators;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Qa.Web.Controllers
{
    [Route(RoutePrefixPaths.VacancyRoutePath)]
    public class ReviewController : Controller
    {
        private readonly ReviewOrchestrator _orchestrator;

        public ReviewController(ReviewOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("review", Name = RouteNames.Vacancy_Review_Get)]
        public async Task<IActionResult> Review([FromRoute] long vacancyReference) 
        {
            var vm = await _orchestrator.GetReviewViewModelAsync(vacancyReference);

            return View(vm);
        }
    }
}
