using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Models.ApplicationReviews;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountApplicationReviewsRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
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
                viewModel.ShouldMakeOthersUnsuccessfulBannerHeader = TempData[TempDataKeys.ApplicationReviewStatusInfoMessage].ToString();
                viewModel.ShouldMakeOthersUnsuccessfulBannerBody = InfoMessages.ApplicationReviewSuccessStatusBannerMessage;
            }

            return View(viewModel);
        }

        [HttpPost("unsuccessful", Name = RouteNames.ApplicationReviewsToUnsuccessful_Post)]
        public async Task<IActionResult> ApplicationReviewsToUnsuccessful(ApplicationReviewsToUnsuccessfulRequest request, [FromQuery] string sortColumn, [FromQuery] string sortOrder)
        {
            Enum.TryParse<SortOrder>(sortOrder, out var outputSortOrder);
            Enum.TryParse<SortColumn>(sortColumn, out var outputSortColumn);

            if (!ModelState.IsValid)
            {
                var viewModel = await _orchestrator.GetApplicationReviewsToUnsuccessfulViewModelAsync(request, outputSortColumn, outputSortOrder);
                return View(viewModel);
            }
            
            await _orchestrator.PostApplicationReviewsStatus
            (
                new ApplicationReviewsToUpdateStatusModel
                {
                    VacancyId = request.VacancyId!.Value!,
                    ApplicationReviewIds = request.ApplicationsToUnsuccessful
                }, 
                User.ToVacancyUser(), 
                null,
                ApplicationReviewStatus.PendingToMakeUnsuccessful
            );
            
            return RedirectToRoute(RouteNames.ApplicationReviewsToUnsuccessfulFeedback_Get, new { request.Ukprn, request.VacancyId });
        }

        [HttpGet("unsuccessful-feedback", Name = RouteNames.ApplicationReviewsToUnsuccessfulFeedback_Get)]
        public IActionResult ApplicationReviewsToUnsuccessfulFeedback(ApplicationReviewsToUnsuccessfulRouteModel request)
        {
            var applicationReviewsToUnsuccessfulFeedbackViewModel = new ApplicationReviewsToUnsuccessfulFeedbackViewModel
            {
                VacancyId = request.VacancyId,
                Ukprn = request.Ukprn
            };
            return View(applicationReviewsToUnsuccessfulFeedbackViewModel);
        }

        [HttpPost("unsuccessful-feedback", Name = RouteNames.ApplicationReviewsToUnsuccessfulFeedback_Post)]
        public async Task<IActionResult> ApplicationReviewsToUnsuccessfulFeedback(ApplicationReviewsToUnsuccessfulFeedbackViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            await _orchestrator.PostApplicationReviewPendingUnsuccessfulFeedback
            (
                new ApplicationReviewStatusModel
                {
                    VacancyId = request.VacancyId!.Value!,
                    CandidateFeedback = request.CandidateFeedback
                }, 
                User.ToVacancyUser(), 
                ApplicationReviewStatus.PendingToMakeUnsuccessful
            );
            
            return RedirectToRoute(RouteNames.ApplicationReviewsToUnsuccessfulConfirmation_Get, new { request.IsMultipleApplications, request.Ukprn, request.VacancyId });
        }

        [HttpGet("unsuccessful-confirmation", Name = RouteNames.ApplicationReviewsToUnsuccessfulConfirmation_Get)]
        public async Task<IActionResult> ApplicationReviewsToUnsuccessfulConfirmation(ApplicationReviewsToUnsuccessfulRouteModel request)
        {
            var applicationReviewsToUnsuccessfulConfirmationViewModel = await _orchestrator.GetApplicationReviewsToUnsuccessfulConfirmationViewModel(request);
            return View(applicationReviewsToUnsuccessfulConfirmationViewModel);
        }

        [HttpPost("unsuccessful-confirmation", Name = RouteNames.ApplicationReviewsToUnsuccessfulConfirmation_Post)]
        public async Task<IActionResult> ApplicationReviewsToUnsuccessfulConfirmation(ApplicationReviewsToUnsuccessfulConfirmationViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            if (request.ApplicationsToUnsuccessfulConfirmed == true)
            {
                await _orchestrator.PostApplicationReviewsToUnsuccessfulAsync(request, User.ToVacancyUser());
                SetApplicationsToUnsuccessfulBannerMessageViaTempData(request.IsMultipleApplications);
                return RedirectToRoute(RouteNames.VacancyManage_Get, new { request.Ukprn, request.VacancyId });
            }

            return RedirectToRoute(RouteNames.VacancyManage_Get, new { request.Ukprn, request.VacancyId });
        }

        [HttpGet("", Name = RouteNames.ApplicationReviewsToShare_Get)]
        public async Task<IActionResult> ApplicationReviewsToShare(VacancyRouteModel rm, [FromQuery] string sortColumn, [FromQuery] string sortOrder)
        {
            Enum.TryParse<SortOrder>(sortOrder, out var outputSortOrder);
            Enum.TryParse<SortColumn>(sortColumn, out var outputSortColumn);

            var viewModel = await _orchestrator.GetApplicationReviewsToShareViewModelAsync(rm, outputSortColumn, outputSortOrder);

            return View(viewModel);
        }

        [HttpPost("", Name = RouteNames.ApplicationReviewsToShare_Post)]
        public async Task<IActionResult> ApplicationReviewsToShare(ApplicationReviewsToShareRouteModel rm, [FromQuery] string sortColumn, [FromQuery] string sortOrder)
        {
            Enum.TryParse<SortOrder>(sortOrder, out var outputSortOrder);
            Enum.TryParse<SortColumn>(sortColumn, out var outputSortColumn);

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetApplicationReviewsToShareViewModelAsync(rm, outputSortColumn, outputSortOrder);
                return View(vm);
            }

            await _orchestrator.PostApplicationReviewsStatus(new ApplicationReviewsToUpdateStatusModel
            {
                VacancyId = rm.VacancyId!.Value!,
                ApplicationReviewIds = rm.ApplicationsToShare
            }, User.ToVacancyUser(),
                null, 
                ApplicationReviewStatus.PendingShared);
            

            return RedirectToRoute(RouteNames.ApplicationReviewsToShareConfirmation_Get, new {rm.Ukprn, rm.VacancyId });
        }

        [HttpGet("share", Name = RouteNames.ApplicationReviewsToShareConfirmation_Get)]
        public async Task<IActionResult> ApplicationReviewsToShareConfirmation(ShareApplicationReviewsRequest request)
        {
            var shareApplicationsConfirmationViewModel = await _orchestrator.GetApplicationReviewsToShareConfirmationViewModel(request);
            return View(shareApplicationsConfirmationViewModel);
        }

        [HttpPost("share", Name = RouteNames.ApplicationReviewsToShareConfirmation_Post)]
        public async Task<IActionResult> ApplicationReviewsToShareConfirmation(ShareApplicationReviewsPostRequest request)
        {
            if (request.ShareApplicationsConfirmed)
            {
                await _orchestrator.PostApplicationReviewsStatus(new ApplicationReviewsToUpdateStatusModel
                {
                    VacancyId = request.VacancyId!.Value!,
                    ApplicationReviewIds = request.ApplicationReviewsToShare
                        .Select(c=>c.ApplicationReviewId)
                        .ToList()
                }, User.ToVacancyUser(), ApplicationReviewStatus.Shared, null);
                SetSharedApplicationsBannerMessageViaTempData(request.SharingMultipleApplications);
                return RedirectToRoute(RouteNames.VacancyManage_Get, new { request.Ukprn, request.VacancyId });
            }

            return RedirectToRoute(RouteNames.VacancyManage_Get, new { request.Ukprn, request.VacancyId });
        }
        private void SetApplicationsToUnsuccessfulBannerMessageViaTempData(bool isMultipleApplications)
        {
            if (!isMultipleApplications)
            {
                TempData.Add(TempDataKeys.ApplicationsToUnsuccessfulHeader, string.Format(InfoMessages.ApplicationEmployerUnsuccessfulHeader));
                return;
            }

            TempData.Add(TempDataKeys.ApplicationsToUnsuccessfulHeader, InfoMessages.ApplicationsToUnsuccessfulBannerHeader);
        }

        private void SetSharedApplicationsBannerMessageViaTempData(bool isMultipleSharedApplications)
        {
            if (!isMultipleSharedApplications)
            {
                TempData.Add(TempDataKeys.SharedSingleApplicationsHeader, string.Format(InfoMessages.SharedSingleApplicationsBannerHeader));
                return;
            }

            TempData.Add(TempDataKeys.SharedMultipleApplicationsHeader, InfoMessages.SharedMultipleApplicationsBannerHeader);
        }
    }
}
