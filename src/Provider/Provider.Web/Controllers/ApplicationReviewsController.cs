﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Models.ApplicationReviews;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.FeatureManagement.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountApplicationReviewsRoutePath)]
    [FeatureGate(FeatureNames.ShareApplicationsFeature)]
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
        public async Task<IActionResult> ApplicationReviewsToShareConfirmation(ShareApplicationReviewsRequest request)
        {
            var shareApplicationsConfirmationViewModel = await _orchestrator.GetApplicationReviewsToShareConfirmationViewModel(request);
            return View(shareApplicationsConfirmationViewModel);
        }

        [HttpPost("share", Name = RouteNames.ApplicationReviewsToShareConfirmation_Post)]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
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