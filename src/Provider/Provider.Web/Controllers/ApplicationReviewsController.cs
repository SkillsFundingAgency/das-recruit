using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Models.ApplicationReviews;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountApplicationReviewsRoutePath)]
    public class ApplicationReviewsController : Controller
    {
        private readonly IApplicationReviewsOrchestrator _orchestrator;

        public ApplicationReviewsController(IApplicationReviewsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("", Name = RouteNames.ApplicationReviewsToShare_Get)]
        public async Task<IActionResult> ApplicationReviews(VacancyRouteModel rm)
        {
            var viewModel = await _orchestrator.GetApplicationReviewsToShareViewModelAsync(rm);

            return View(viewModel);
        }

        [HttpPost("", Name = RouteNames.ApplicationReviewsToShare_Post)]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        public IActionResult ApplicationReviewsToShare(ApplicationReviewsToShareRouteModel rm)
        {
            return RedirectToAction(nameof(ApplicationReviewsToShareConfirmation), new { rm.ApplicationsToShare, rm.Ukprn, rm.VacancyId });
        }

        [HttpGet("share", Name = RouteNames.ApplicationReviewsToShareConfirmation_Get)]
        public async Task<IActionResult> ApplicationReviewsToShareConfirmation(ShareMultipleApplicationsRequest request)
        {
            var shareApplicationsConfirmationViewModel = await _orchestrator.GetApplicationReviewsToShareConfirmationViewModel(request);
            return View(shareApplicationsConfirmationViewModel);
        }
    }
}
