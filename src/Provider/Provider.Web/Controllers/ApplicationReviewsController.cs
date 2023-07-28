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
        private const string TempApplicationsToUnSuccessful = "ApplicationsToUnSuccessful";

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
        public IActionResult ApplicationReviewsToUnsuccessful(ApplicationReviewsToUnSuccessfulRequest request)
        {
            return RedirectToAction(nameof(ApplicationReviewsToUnsuccessfulFeedBack), new { request.ApplicationsToUnSuccessful, request.Ukprn, request.VacancyId });
        }

        [HttpGet("unsuccessful-feedback", Name = RouteNames.ApplicationReviewsToUnSuccessfulFeedback_Get)]
        [FeatureGate(FeatureNames.MultipleApplicationsManagement)]
        public IActionResult ApplicationReviewsToUnsuccessfulFeedBack(ApplicationReviewsToUnSuccessfulRouteModel request)
        {
            var applicationReviewsToUnsuccessfulFeedBackViewModel = new ApplicationReviewsToUnsuccessfulFeedBackViewModel
            {
                VacancyId = request.VacancyId,
                Ukprn = request.Ukprn,
                ApplicationsToUnSuccessful = request.ApplicationsToUnSuccessful,
                Outcome = ApplicationReviewStatus.Unsuccessful,
                TargetView = NavigationTarget.MultipleApplicationsUnsuccessfulConfirmation
            };
            return View(applicationReviewsToUnsuccessfulFeedBackViewModel);
        }

        [HttpPost("unsuccessful-feedback", Name = RouteNames.ApplicationReviewsToUnSuccessfulFeedback_Post)]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        [FeatureGate(FeatureNames.MultipleApplicationsManagement)]
        public IActionResult ApplicationReviewsToUnsuccessfulFeedBack(ApplicationReviewsToUnsuccessfulFeedBackViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }
            TempData[TempApplicationsToUnSuccessful] = JsonConvert.SerializeObject(request);
            return RedirectToAction(nameof(ApplicationReviewsToUnSuccessfulConfirmation), new { request.Ukprn, request.VacancyId });
        }

        [HttpGet("unsuccessful-confirmation", Name = RouteNames.ApplicationReviewsToUnSuccessfulConfirmation_Get)]
        [FeatureGate(FeatureNames.MultipleApplicationsManagement)]
        public async Task<IActionResult> ApplicationReviewsToUnSuccessfulConfirmation(ApplicationReviewsToUnSuccessfulRouteModel request)
        {
            if (TempData[TempApplicationsToUnSuccessful] is string model)
            {
                var applicationReviewsStatusChangeModel = JsonConvert.DeserializeObject<ApplicationReviewsStatusChangeModel>(model);
                var applicationReviewsToUnSuccessfulConfirmationViewModel = await _orchestrator.GetApplicationReviewsToUnSuccessfulConfirmationViewModel(applicationReviewsStatusChangeModel);
                return View(applicationReviewsToUnSuccessfulConfirmationViewModel);
            }

            return RedirectToAction(nameof(ApplicationReviewsToUnsuccessful), new { request.Ukprn, request.VacancyId });
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
