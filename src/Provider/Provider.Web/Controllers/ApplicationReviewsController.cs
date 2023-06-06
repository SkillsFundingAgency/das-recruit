using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("", Name = RouteNames.ApplicationReviewsToShare_Get)]
        public async Task<IActionResult> ApplicationReviews(VacancyRouteModel rm)
        {
            var viewModel = await _orchestrator.GetApplicationReviewsToShareWithEmployerViewModelAsync(rm);

            return View(viewModel);
        }

        // The Continue button is hit when multiple applications slected via the checkbox
        // this redirects to a GET ApplicationRevewsToShareConfirmation
        [HttpPost("", Name = RouteNames.ApplicationReviewsToShare_Post)]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        public IActionResult ApplicationReviewsToShare(ApplicationReviewsToShareRouteModel rm)
        {
            return RedirectToAction(nameof(ApplicationReviewsToShareConfirmation), new { rm.ApplicationsToShare, rm.Ukprn, rm.VacancyId });
        }

        // GET ApplicationReviewsToShareConfirmation
        // To hold the list of ApplicationReviewIds that need their Status changed to SHARED
        [HttpGet("share", Name = RouteNames.ApplicationReviewsToShareConfirmation_Get)]
        public async Task<IActionResult> ApplicationReviewsToShareConfirmation(ShareMultipleApplicationsPostRequest request)
        {
            var shareApplicationsConfirmationViewModel = await _orchestrator.GetApplicationReviewsToShareConfirmationViewModel(request);
            return View(shareApplicationsConfirmationViewModel);
        }
    }
}
