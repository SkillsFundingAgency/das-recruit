using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountApplicationReviewsRoutePath)]
    public class ApplicationReviewsController : Controller
    {
        private readonly ApplicationReviewsOrchestrator _orchestrator;

        public ApplicationReviewsController(ApplicationReviewsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("", Name = RouteNames.ApplicationReviewsToShareWithEmployer_Get)]
        public async Task<IActionResult> ApplicationReviews(VacancyRouteModel rm)
        {
            var vm = await _orchestrator.GetApplicationReviewsToShareWithEmployerViewModelAsync(rm);
            return View(vm);
        }
    }
}
