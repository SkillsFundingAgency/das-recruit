﻿using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReviews;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountApplicationReviewsRoutePath)]
    public class ApplicationReviewsController : Controller
    {
        private readonly IApplicationReviewsOrchestrator _orchestrator;

        public ApplicationReviewsController(IApplicationReviewsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [FeatureGate(FeatureNames.MultipleApplicationsManagement)]
        [HttpGet("unsuccessful", Name = RouteNames.ApplicationReviewsToUnsuccessful_Get)]
        public async Task<IActionResult> ApplicationReviewsToUnsuccessful(VacancyRouteModel rm)
        {
            var viewModel = await _orchestrator.GetApplicationReviewsToUnsuccessfulViewModelAsync(rm);

            return View(viewModel);
        }

        [FeatureGate(FeatureNames.MultipleApplicationsManagement)]
        [HttpPost("unsuccessful", Name = RouteNames.ApplicationReviewsToUnsuccessful_Post)]
        public async Task<IActionResult> ApplicationReviewsToUnsuccessfulAsync(ApplicationReviewsToUnsuccessfulRouteModel rm)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = await _orchestrator.GetApplicationReviewsToUnsuccessfulViewModelAsync(rm);
                return View(viewModel);
            }

            return RedirectToAction(nameof(ApplicationReviewsFeedback), new { rm.ApplicationsToUnsuccessful, rm.EmployerAccountId, rm.VacancyId });
        }

        [FeatureGate(FeatureNames.MultipleApplicationsManagement)]
        [HttpGet("unsuccessful-feedback", Name = RouteNames.ApplicationReviewsToUnsuccessfulFeedback_Get)]
        public IActionResult ApplicationReviewsFeedback(ApplicationReviewsToUnsuccessfulRouteModel request)
        {
            var viewModel = _orchestrator.GetApplicationReviewsFeedbackViewModel(request);

            return View(viewModel);
        }

        [FeatureGate(FeatureNames.MultipleApplicationsManagement)]
        [HttpPost("unsuccessful-feedback", Name = RouteNames.ApplicationReviewsToUnsuccessfulFeedback_Post)]
        public IActionResult ApplicationReviewsFeedback(ApplicationReviewsFeedbackViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            return RedirectToAction(nameof(ApplicationReviewsToUnsuccessfulConfirmation), new
            {
                request.Outcome,
                request.ApplicationsToUnsuccessful,
                request.CandidateFeedback,
                request.IsMultipleApplications,
                request.EmployerAccountId,
                request.VacancyId
            });
        }

        [FeatureGate(FeatureNames.MultipleApplicationsManagement)]
        [HttpGet("unsuccessful-confirmation", Name = RouteNames.ApplicationReviewsToUnsuccessfulConfirmation_Get)]
        public async Task<IActionResult> ApplicationReviewsToUnsuccessfulConfirmation(ApplicationReviewsToUnsuccessfulRouteModel rm)
        {
            var viewModel = await _orchestrator.GetApplicationReviewsToUnsuccessfulConfirmationViewModelAsync(rm);

            return View(viewModel);
        }
    }
}
