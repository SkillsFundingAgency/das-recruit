using System;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Configuration.Routing;
using Esfa.Recruit.Qa.Web.Orchestrators;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Qa.Web.Controllers
{
    [Route(RoutePrefixPaths.VacancyReviewsRoutePath)]
    public class ReviewController : Controller
    {
        private readonly ReviewOrchestrator _orchestrator;

        public ReviewController(ReviewOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet(Name = RouteNames.Vacancy_Review_Get)]
        public async Task<IActionResult> Review([FromRoute] Guid reviewId) 
        {
            var vm = await _orchestrator.GetReviewViewModelAsync(reviewId);

            return View(vm);
        }

        [HttpPost(Name = RouteNames.Vacancy_Review_Post)]
        public async Task<IActionResult> Submit([FromRoute] Guid reviewId) 
        {
            await _orchestrator.ApproveReviewAsync(reviewId);

            return RedirectToRoute(RouteNames.Dashboard_Index_Get);
        }
    }
}
