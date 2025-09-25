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
using InfoMsg = Esfa.Recruit.Shared.Web.ViewModels.InfoMessages;
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

        [HttpGet("unsuccessful", Name = RouteNames.ApplicationReviewsToUnsuccessful_Get)]
        public async Task<IActionResult> ApplicationReviewsToUnsuccessful(VacancyRouteModel rm, [FromQuery] string sortColumn, [FromQuery] string sortOrder)
        {
            Enum.TryParse<SortOrder>(sortOrder, out var outputSortOrder);
            Enum.TryParse<SortColumn>(sortColumn, out var outputSortColumn);

            var viewModel = await _orchestrator.GetApplicationReviewsToUnsuccessfulViewModelAsync(rm, outputSortColumn, outputSortOrder);

            if (TempData.ContainsKey(TempDataKeys.ApplicationReviewStatusInfoMessage)) 
            {
                viewModel.PositionsFilledBannerHeader = TempData[TempDataKeys.ApplicationReviewStatusInfoMessage].ToString();
                viewModel.PositionsFilledBannerBody = InfoMsg.ApplicationReviewSuccessStatusBannerMessage;
            }

            return View(viewModel);
        }

        [HttpPost("unsuccessful", Name = RouteNames.ApplicationReviewsToUnsuccessful_Post)]
        public async Task<IActionResult> ApplicationReviewsToUnsuccessfulAsync(ApplicationReviewsToUnsuccessfulViewModel rm)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = await _orchestrator.GetApplicationReviewsToUnsuccessfulViewModelAsync(rm, rm.SortColumn, rm.SortOrder);
                return View(viewModel);
            }
            await _orchestrator.PostApplicationReviewsStatus
            (
                new ApplicationReviewsToUpdateStatusModel
                {
                    VacancyId = rm.VacancyId,
                    ApplicationReviewIds = rm.ApplicationsToUnsuccessful
                }, 
                User.ToVacancyUser(), 
                null,
                ApplicationReviewStatus.PendingToMakeUnsuccessful
            );

            return RedirectToRoute(RouteNames.ApplicationReviewsToUnsuccessfulFeedback_Get, new {rm.EmployerAccountId, rm.VacancyId });
        }

        [HttpGet("unsuccessful-feedback", Name = RouteNames.ApplicationReviewsToUnsuccessfulFeedback_Get)]
        public async Task<IActionResult> ApplicationReviewsFeedback(ApplicationReviewsToUnsuccessfulRouteModel request)
        {
            var viewModel = await _orchestrator.GetApplicationReviewsFeedbackViewModel(request);

            return View(viewModel);
        }

        [HttpPost("unsuccessful-feedback", Name = RouteNames.ApplicationReviewsToUnsuccessfulFeedback_Post)]
        public async Task<IActionResult> ApplicationReviewsFeedback(ApplicationReviewsFeedbackViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }
            await _orchestrator.PostApplicationReviewPendingUnsuccessfulFeedback
            (
                new ApplicationReviewStatusModel
                {
                    VacancyId = request.VacancyId!,
                    CandidateFeedback = request.CandidateFeedback
                }, 
                User.ToVacancyUser(), 
                ApplicationReviewStatus.PendingToMakeUnsuccessful
            );

            return RedirectToRoute(RouteNames.ApplicationReviewsToUnsuccessfulConfirmation_Get, new
            {
                request.IsMultipleApplications,
                request.EmployerAccountId,
                request.VacancyId
            });
        }

        [HttpGet("unsuccessful-confirmation", Name = RouteNames.ApplicationReviewsToUnsuccessfulConfirmation_Get)]
        public async Task<IActionResult> ApplicationReviewsToUnsuccessfulConfirmation(ApplicationReviewsToUnsuccessfulRouteModel rm)
        {
            var viewModel = await _orchestrator.GetApplicationReviewsToUnsuccessfulConfirmationViewModelAsync(rm);

            return View(viewModel);
        }

        [HttpPost("unsuccessful-confirmation", Name = RouteNames.ApplicationReviewsToUnsuccessfulConfirmation_Post)]
        public async Task<IActionResult> ApplicationReviewsToUnsuccessfulConfirmation(ApplicationReviewsToUnsuccessfulConfirmationViewModel rm)
        {
            if (!ModelState.IsValid)
            {
                return View(rm);
            }

            if (rm.ApplicationsUnsuccessfulConfirmed == true)
            {
                await _orchestrator.PostApplicationReviewsToUnsuccessfulAsync(rm, User.ToVacancyUser());
                SetApplicationsReviewsToUnsuccessfulBannerMessage(rm);
                return RedirectToRoute(RouteNames.VacancyManage_Get, new { rm.EmployerAccountId, rm.VacancyId });
            }

            return RedirectToRoute(RouteNames.VacancyManage_Get, new { rm.EmployerAccountId, rm.VacancyId });
        }
        private void SetApplicationsReviewsToUnsuccessfulBannerMessage(ApplicationReviewsToUnsuccessfulConfirmationViewModel model)
        {
            if (model.IsMultipleApplications) 
            {
                TempData.Add(TempDataKeys.ApplicationReviewsUnsuccessfulInfoMessage, InfoMsg.ApplicationsToUnsuccessfulBannerHeader);
                return;
            }
            TempData.Add(TempDataKeys.ApplicationReviewsUnsuccessfulInfoMessage, string.Format(InfoMsg.ApplicationReviewUnsuccessStatusHeader));
        }
    }
}
