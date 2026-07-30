using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using InfoMsg = Esfa.Recruit.Shared.Web.ViewModels.InfoMessages;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountApplicationReviewsRoutePath)]
    public class ApplicationReviewsController(IApplicationReviewsOrchestrator orchestrator) : Controller
    {
        [HttpGet("unsuccessful", Name = RouteNames.ApplicationReviewsToUnsuccessful_Get)]
        public async Task<IActionResult> ApplicationReviewsToUnsuccessful(VacancyRouteModel rm, [FromQuery] string sortColumn, [FromQuery] string sortOrder)
        {
            Enum.TryParse<SortOrder>(sortOrder, out var outputSortOrder);
            Enum.TryParse<SortColumn>(sortColumn, out var outputSortColumn);

            var viewModel = await orchestrator.GetApplicationReviewsToUnsuccessfulViewModelAsync(rm, outputSortColumn, outputSortOrder);

            if (TempData.ContainsKey(TempDataKeys.ApplicationReviewStatusInfoMessage)) 
            {
                viewModel.PositionsFilledBannerHeader = TempData[TempDataKeys.ApplicationReviewStatusInfoMessage].ToString();
                viewModel.PositionsFilledBannerBody = InfoMsg.ApplicationReviewSuccessStatusBannerMessage;
            }

            return View(viewModel);
        }

        [HttpPost("unsuccessful", Name = RouteNames.ApplicationReviewsToUnsuccessful_Post)]
        [RequestFormLimits(ValueCountLimit = 4096)]
        public async Task<IActionResult> ApplicationReviewsToUnsuccessfulAsync(ApplicationReviewsToUnsuccessfulViewModel rm)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = await orchestrator.GetApplicationReviewsToUnsuccessfulViewModelAsync(rm, rm.SortColumn, rm.SortOrder);
                return View(viewModel);
            }
            await orchestrator.PostApplicationReviewsStatus
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
            var viewModel = await orchestrator.GetApplicationReviewsFeedbackViewModel(request);

            return View(viewModel);
        }

        [HttpPost("unsuccessful-feedback", Name = RouteNames.ApplicationReviewsToUnsuccessfulFeedback_Post)]
        [RequestFormLimits(ValueCountLimit = 4096)]
        public async Task<IActionResult> ApplicationReviewsFeedback(ApplicationReviewsFeedbackViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var confirmationModel = new ApplicationReviewsToUnsuccessfulConfirmationViewModel
            {
                CandidateFeedback = request.CandidateFeedback,
                ApplicationsUnsuccessfulConfirmed = true,
                VacancyApplicationsToUnsuccessful = request.ApplicationsToUnsuccessful,
                Outcome = request.Outcome,
                VacancyId = request.VacancyId,
                EmployerAccountId = request.EmployerAccountId,
            };

            await orchestrator.PostApplicationReviewsToUnsuccessfulAsync(confirmationModel, User.ToVacancyUser());

            var routeValues = new { request.EmployerAccountId, request.VacancyId };

            if (await orchestrator.IsAllApplicationReviewsHasOutcomeAsync(request.VacancyId))
            {
                TempData.TryAdd(TempDataKeys.ArchiveAdvertInfoMessage, InfoMessages.AdvertApplicantsOutcomeNotified);
                return RedirectToRoute(RouteNames.ArchiveVacancy_Get, routeValues);
            }

            SetApplicationsReviewsToUnsuccessfulBannerMessage(request.ApplicationsToUnsuccessful.Count > 1);

            return RedirectToRoute(RouteNames.VacancyManage_Get, routeValues);
        }

        private void SetApplicationsReviewsToUnsuccessfulBannerMessage(bool isMultipleApplications)
        {
            if (isMultipleApplications) 
            {
                TempData.Add(TempDataKeys.ApplicationReviewsUnsuccessfulInfoMessage, InfoMsg.ApplicationsEmployerUnsuccessfulHeader);
                return;
            }
            TempData.Add(TempDataKeys.ApplicationReviewsUnsuccessfulInfoMessage, InfoMsg.ApplicationEmployerUnsuccessfulHeader);
        }
    }
}
