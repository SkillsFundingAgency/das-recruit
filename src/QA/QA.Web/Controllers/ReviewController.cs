using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Configuration.Routing;
using Esfa.Recruit.Qa.Web.Extensions;
using Esfa.Recruit.Qa.Web.Orchestrators;
using Esfa.Recruit.Qa.Web.Security;
using Esfa.Recruit.Qa.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Qa.Web.Controllers
{
    [Route(RoutePaths.VacancyReviewsRoutePath)]
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
            // if the user is not authenticated, redirect them back to start now page.
            if (User.Identity is {IsAuthenticated: false}) return RedirectToAction("Index", "Home");

            var vm = await _orchestrator.GetReviewViewModelAsync(reviewId, User.GetVacancyUser());
          
            return View(vm);
        }

        [HttpPost(Name = RouteNames.Vacancy_Review_Post)]
        public async Task<IActionResult> Submit(ReviewEditModel model) 
        {
            if (ModelState.IsValid == false)
            {
                if (model.SelectedAutomatedQaResults.Any() || model.SelectedFieldIdentifiers.Any() || model.IsRefer)
                {
                    var vm = await _orchestrator.GetReviewViewModelAsync(model, User.GetVacancyUser());
                    return View("Review", vm);    
                }
                ModelState.ClearValidationState(nameof(ReviewEditModel.ReviewerComment));
            }

            var nextVacancyReviewId = await _orchestrator.SubmitReviewAsync(model, User.GetVacancyUser());

            if (nextVacancyReviewId == null)
                return RedirectToRoute(RouteNames.Dashboard_Index_Get);

            return RedirectToRoute(RouteNames.Vacancy_Review_Get, new { reviewId = nextVacancyReviewId });
        }

        [Authorize(Policy = AuthorizationPolicyNames.TeamLeadUserPolicyName)]
        [HttpGet("unassign", Name = RouteNames.Vacancy_Review_Unassign_Get)]
        public async Task<IActionResult> UnassignReview([FromRoute] Guid reviewId)
        {
            var unassignReviewVM = await _orchestrator.GetUnassignReviewViewModelAsync(reviewId);
            return View(unassignReviewVM);
        }

        [Authorize(Policy = AuthorizationPolicyNames.TeamLeadUserPolicyName)]
        [HttpPost("unassign", Name = RouteNames.Vacancy_Review_Unassign_Post)]
        public async Task<IActionResult> UnassignReview(UnassignReviewEditModel model)
        {
            var unassignReviewVM = await _orchestrator.GetUnassignReviewViewModelAsync(model.ReviewId);
            if (!ModelState.IsValid)
                return View(unassignReviewVM);
            if (model.ConfirmUnassign.GetValueOrDefault())
                await _orchestrator.UnassignVacancyReviewAsync(model.ReviewId);
            return RedirectToRoute(RouteNames.Dashboard_Index_Get);
        }

        [HttpGet("readonly", Name = RouteNames.Vacancy_Readonly_Review_Get)]
        public async Task<IActionResult> ReadonlyReview([FromRoute] Guid reviewId)
        {
            var vm = await _orchestrator.GetReadonlyReviewViewModelAsync(reviewId);

            return View(vm);
        }
    }
}
