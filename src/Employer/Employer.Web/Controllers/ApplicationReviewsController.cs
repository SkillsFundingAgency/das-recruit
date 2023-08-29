using System.Collections.Generic;
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
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

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
        public async Task<IActionResult> ApplicationReviewsToUnsuccessful(VacancyRouteModel rm, [FromQuery] string sortColumn, [FromQuery] string sortOrder)
        {
            Enum.TryParse<SortOrder>(sortOrder, out var outputSortOrder);
            Enum.TryParse<SortColumn>(sortColumn, out var outputSortColumn);

            var viewModel = await _orchestrator.GetApplicationReviewsToUnsuccessfulViewModelAsync(rm, outputSortColumn, outputSortOrder);

            return View(viewModel);
        }

        [FeatureGate(FeatureNames.MultipleApplicationsManagement)]
        [HttpPost("unsuccessful", Name = RouteNames.ApplicationReviewsToUnsuccessful_Post)]
        public async Task<IActionResult> ApplicationReviewsToUnsuccessfulAsync(ApplicationReviewsToUnsuccessfulRouteModel rm, [FromQuery] string sortColumn, [FromQuery] string sortOrder)
        {
            Enum.TryParse<SortOrder>(sortOrder, out var outputSortOrder);
            Enum.TryParse<SortColumn>(sortColumn, out var outputSortColumn);

            if (!ModelState.IsValid)
            {
                var viewModel = await _orchestrator.GetApplicationReviewsToUnsuccessfulViewModelAsync(rm, outputSortColumn, outputSortOrder);
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

        [HttpPost("unsuccessful-confirmation", Name = RouteNames.ApplicationReviewsToUnsuccessfulConfirmation_Post)]
        [FeatureGate(FeatureNames.MultipleApplicationsManagement)]
        public async Task<IActionResult> ApplicationReviewsToUnsuccessfulConfirmation(ApplicationReviewsToUnsuccessfulConfirmationViewModel rm)
        {
            if (!ModelState.IsValid)
            {
                return View(rm);
            }

            if (rm.ApplicationsUnsuccessfulConfirmed == true)
            {
                await _orchestrator.PostApplicationReviewsToUnsuccessfulAsync(rm, User.ToVacancyUser());
                SetApplicationsReviewsToUnsuccessfulBannerMessage();
                return RedirectToRoute(RouteNames.VacancyManage_Get, new { rm.EmployerAccountId, rm.VacancyId });
            }

            return RedirectToRoute(RouteNames.VacancyManage_Get, new { rm.EmployerAccountId, rm.VacancyId });
        }
        private void SetApplicationsReviewsToUnsuccessfulBannerMessage()
        {
            TempData.Add(TempDataKeys.ApplicationReviewsUnsuccessfulInfoMessage, InfoMessages.ApplicationsToUnsuccessfulBannerHeader);
            return;
        }
    }
}
