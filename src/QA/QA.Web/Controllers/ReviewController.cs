using System;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Configuration.Routing;
using Esfa.Recruit.Qa.Web.Extensions;
using Esfa.Recruit.Qa.Web.Orchestrators;
using Esfa.Recruit.Qa.Web.ViewModels;
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
            var vm = await _orchestrator.GetReviewViewModelAsync(reviewId, User.GetVacancyUser());

            return View(vm);
        }

        [HttpPost(Name = RouteNames.Vacancy_Review_Post)]
        public async Task<IActionResult> Approve([FromRoute] Guid reviewId) 
        {
            await _orchestrator.ApproveReviewAsync(reviewId);

            return RedirectToRoute(RouteNames.Dashboard_Index_Get);
        }

        [HttpGet("referral", Name = RouteNames.Vacancy_Review_Referral_Get)]
        public async Task<IActionResult> Referral([FromRoute] Guid reviewId) 
        {
            var vm = await _orchestrator.GetReferralViewModelAsync(reviewId);

            return View("Review", vm);
        }

        [HttpPost("referral", Name = RouteNames.Vacancy_Review_Referral_Post)]
        public async Task<IActionResult> ReferralApprove([FromRoute] Guid reviewId, ReferralViewModel reviewChanges) 
        {
            await _orchestrator.ApproveReferredReviewAsync(reviewId, reviewChanges);

            return RedirectToRoute(RouteNames.Dashboard_Index_Get);
        }
    }
}
