using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Models.ApplicationReviews;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.FeatureManagement.Mvc;
using Newtonsoft.Json;

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

        [HttpGet("unsuccessful", Name = RouteNames.ApplicationReviewsToUnsuccessful_Get)]
        [FeatureGate(FeatureNames.MultipleApplicationsManagement)]
        public async Task<IActionResult> ApplicationReviewsToUnsuccessful(VacancyRouteModel rm)
        {
            var viewModel = await _orchestrator.GetApplicationReviewsToUnsuccessfulViewModelAsync(rm);

            return View(viewModel);
        }

        [HttpPost("unsuccessful", Name = RouteNames.ApplicationReviewsToUnsuccessful_Post)]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        [FeatureGate(FeatureNames.MultipleApplicationsManagement)]
        public async Task<IActionResult> ApplicationReviewsToUnsuccessful(ApplicationReviewsToUnSuccessfulRequest request)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = await _orchestrator.GetApplicationReviewsToUnsuccessfulViewModelAsync(request);
                return View(viewModel);
            }
            return RedirectToAction(nameof(ApplicationReviewsToUnsuccessfulFeedBack), new { request.ApplicationsToUnSuccessful, request.Ukprn, request.VacancyId });
        }

        [HttpGet("unsuccessful-feedback", Name = RouteNames.ApplicationReviewsToUnsuccessfulFeedback_Get)]
        [FeatureGate(FeatureNames.MultipleApplicationsManagement)]
        public IActionResult ApplicationReviewsToUnsuccessfulFeedback(ApplicationReviewsToUnsuccessfulRouteModel request)
        {
            var applicationReviewsToUnsuccessfulFeedbackViewModel = new ApplicationReviewsToUnsuccessfulFeedbackViewModel
            {
                VacancyId = request.VacancyId,
                Ukprn = request.Ukprn,
                ApplicationsToUnsuccessful = request.ApplicationsToUnsuccessful,
                Outcome = ApplicationReviewStatus.Unsuccessful
            };
            return View(applicationReviewsToUnsuccessfulFeedbackViewModel);
        }

        [HttpPost("unsuccessful-feedback", Name = RouteNames.ApplicationReviewsToUnsuccessfulFeedback_Post)]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        [FeatureGate(FeatureNames.MultipleApplicationsManagement)]
        public IActionResult ApplicationReviewsToUnsuccessfulFeedback(ApplicationReviewsToUnsuccessfulFeedbackViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            return RedirectToAction(nameof(ApplicationReviewsToUnsuccessfulConfirmation), new { request.Outcome, 
                request.ApplicationsToUnsuccessful, request.CandidateFeedback, request.IsMultipleApplications, request.Ukprn, request.VacancyId });
        }

        [HttpGet("unsuccessful-confirmation", Name = RouteNames.ApplicationReviewsToUnsuccessfulConfirmation_Get)]
        [FeatureGate(FeatureNames.MultipleApplicationsManagement)]
        public async Task<IActionResult> ApplicationReviewsToUnsuccessfulConfirmation(ApplicationReviewsToUnsuccessfulRouteModel request)
        {
            var applicationReviewsToUnsuccessfulConfirmationViewModel = await _orchestrator.GetApplicationReviewsToUnsuccessfulConfirmationViewModel(request);
            return View(applicationReviewsToUnsuccessfulConfirmationViewModel);
        }

        [HttpGet("", Name = RouteNames.ApplicationReviewsToShare_Get)]
        [FeatureGate(FeatureNames.ShareApplicationsFeature)]
        public async Task<IActionResult> ApplicationReviewsToShare(VacancyRouteModel rm)
        {
            var viewModel = await _orchestrator.GetApplicationReviewsToShareViewModelAsync(rm);

            return View(viewModel);
        }

        [HttpPost("", Name = RouteNames.ApplicationReviewsToShare_Post)]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        [FeatureGate(FeatureNames.ShareApplicationsFeature)]
        public IActionResult ApplicationReviewsToShare(ApplicationReviewsToShareRouteModel rm)
        {
            return RedirectToAction(nameof(ApplicationReviewsToShareConfirmation), new { rm.ApplicationsToShare, rm.Ukprn, rm.VacancyId });
        }

        [HttpGet("share", Name = RouteNames.ApplicationReviewsToShareConfirmation_Get)]
        [FeatureGate(FeatureNames.ShareApplicationsFeature)]
        public async Task<IActionResult> ApplicationReviewsToShareConfirmation(ShareApplicationReviewsRequest request)
        {
            var shareApplicationsConfirmationViewModel = await _orchestrator.GetApplicationReviewsToShareConfirmationViewModel(request);
            return View(shareApplicationsConfirmationViewModel);
        }

        [HttpPost("share", Name = RouteNames.ApplicationReviewsToShareConfirmation_Post)]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        [FeatureGate(FeatureNames.ShareApplicationsFeature)]
        public async Task<IActionResult> ApplicationReviewsToShareConfirmation(ShareApplicationReviewsPostRequest request)
        {
            if (request.ShareApplicationsConfirmed)
            {
                await _orchestrator.PostApplicationReviewsStatusConfirmationAsync(request, User.ToVacancyUser());
                SetSharedApplicationsBannerMessageViaTempData(request.ApplicationReviewsToShare);
                return RedirectToRoute(RouteNames.VacancyManage_Get, new { request.Ukprn, request.VacancyId });
            }

            return RedirectToRoute(RouteNames.VacancyManage_Get, new { request.Ukprn, request.VacancyId });
        }

        private void SetSharedApplicationsBannerMessageViaTempData(List<VacancyApplication> sharedApplications)
        {
            if (!sharedApplications.Any())
                return;

            if (sharedApplications.Count() == 1)
            {
                TempData.Add(TempDataKeys.SharedSingleApplicationsHeader, string.Format(InfoMessages.SharedSingleApplicationsBannerHeader, sharedApplications.First().CandidateName));
                return;
            }

            TempData.Add(TempDataKeys.SharedMultipleApplicationsHeader, InfoMessages.SharedMultipleApplicationsBannerHeader);
            return;
        }
    }
}
