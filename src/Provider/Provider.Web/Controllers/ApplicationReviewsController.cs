using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews;
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
            var viewModel = await _orchestrator.GetApplicationReviewsToShareWithEmployerViewModelAsync(rm);

            return View(viewModel);
        }

        [HttpPost("", Name = RouteNames.ApplicationReviewsToShare_Post)]
        public async Task<IActionResult> ApplicationReviewsToShare(ShareMultipleApplicationsPostRequest rm)
        {
            if (!ModelState.IsValid)
            {
                // todo
            }

            // reroute to ApplicationReviewsToShareConfirmation action passing on an ApplicationReviewsToShareRouteModel

            return View();
        }
    }
}
