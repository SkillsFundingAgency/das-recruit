using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Models.ApplicationReviews;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountApplicationReviewRoutePath)]
    public class ApplicationReviewController(IApplicationReviewOrchestrator orchestrator) : Controller
    {
        private const string TempDateArModel = "ApplicationReviewEditModel";

        [HttpGet("", Name = RouteNames.ApplicationReview_Get)]
        public async Task<IActionResult> ApplicationReview(ApplicationReviewRouteModel rm)
        {
            var vm = await orchestrator.GetApplicationReviewViewModelAsync(rm);
            var viewName = vm.IsFaaV2Application ? "ApplicationReviewV2" : "ApplicationReview";
            return View(viewName, vm);
        }

        [HttpPost("", Name = RouteNames.ApplicationReview_Post)]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        public async Task<IActionResult> ApplicationReview(ApplicationReviewEditModel applicationReviewEditModel)
        {
            if (!ModelState.IsValid)
            {
                var vm = await orchestrator.GetApplicationReviewViewModelAsync(applicationReviewEditModel);
                var viewName = vm.IsFaaV2Application ? "ApplicationReviewV2" : "ApplicationReview";
                return View(viewName, vm);
            }

            switch (applicationReviewEditModel.Outcome.GetValueOrDefault())
            {
                case ApplicationReviewStatus.Shared:
                    var shareApplicationsModel = new ShareApplicationReviewsRequest
                    {
                        Ukprn = applicationReviewEditModel.Ukprn,
                        VacancyId = applicationReviewEditModel.VacancyId,
                        ApplicationsToShare = [applicationReviewEditModel.ApplicationReviewId]
                    };
                    return RedirectToRoute(RouteNames.ApplicationReviewsToShareConfirmation_Get,
                        shareApplicationsModel);

                case ApplicationReviewStatus.InReview:
                case ApplicationReviewStatus.Interviewing:
                    var statusChangeInfo =
                        await orchestrator.PostApplicationReviewStatusChangeModelAsync(applicationReviewEditModel,
                            User.ToVacancyUser());
                    TempData.Add(TempDataKeys.ApplicationStatusChangedHeader,
                        string.Format(InfoMessages.ApplicationStatusChangeBannerHeader, statusChangeInfo.CandidateName,
                            applicationReviewEditModel.Outcome.GetDisplayName().ToLower()));
                    return RedirectToRoute(RouteNames.VacancyManage_Get,
                        new {applicationReviewEditModel.VacancyId, applicationReviewEditModel.Ukprn});

                case ApplicationReviewStatus.Successful:
                    TempData[TempDateArModel] = JsonConvert.SerializeObject(applicationReviewEditModel);
                    return RedirectToRoute(RouteNames.ApplicationReviewConfirmation_Get,
                        new
                        {
                            applicationReviewEditModel.ApplicationReviewId, applicationReviewEditModel.VacancyId,
                            applicationReviewEditModel.Ukprn
                        });

                case ApplicationReviewStatus.EmployerUnsuccessful:
                    applicationReviewEditModel.Outcome = ApplicationReviewStatus.Unsuccessful;
                    TempData[TempDateArModel] = JsonConvert.SerializeObject(applicationReviewEditModel);
                    return RedirectToRoute(RouteNames.ApplicationReviewConfirmation_Get,
                        new
                        {
                            applicationReviewEditModel.ApplicationReviewId, applicationReviewEditModel.VacancyId,
                            applicationReviewEditModel.Ukprn
                        });

                case ApplicationReviewStatus.Unsuccessful:
                    TempData[TempDateArModel] = JsonConvert.SerializeObject(applicationReviewEditModel);
                    return RedirectToRoute(RouteNames.ApplicationReviewFeedback_Get,
                        new
                        {
                            applicationReviewEditModel.ApplicationReviewId, applicationReviewEditModel.VacancyId,
                            applicationReviewEditModel.Ukprn
                        });

                default:
                    var vm = await orchestrator.GetApplicationReviewViewModelAsync(applicationReviewEditModel);
                    return View(vm);
            }
        }

        [HttpGet("feedback", Name = RouteNames.ApplicationReviewFeedback_Get)]
        public async Task<IActionResult> ApplicationFeedback(ApplicationReviewRouteModel applicationReviewEditModel)
        {
            if (TempData[TempDateArModel] is string model)
            {
                var applicationReviewEditViewModel = JsonConvert.DeserializeObject<ApplicationReviewEditModel>(model);
                var applicationReviewFeedbackViewModel =
                    await orchestrator.GetApplicationReviewFeedbackViewModelAsync(applicationReviewEditViewModel);
                return View(applicationReviewFeedbackViewModel);
            }

            return RedirectToRoute(RouteNames.ApplicationReview_Get,
                new
                {
                    applicationReviewEditModel.ApplicationReviewId, applicationReviewEditModel.VacancyId,
                    applicationReviewEditModel.Ukprn
                });
        }

        [HttpPost("feedback", Name = RouteNames.ApplicationReviewFeedback_Post)]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        public async Task<IActionResult> ApplicationFeedback(
            ApplicationReviewFeedbackViewModel applicationReviewFeedbackEditModel)
        {
            if (!ModelState.IsValid)
            {
                applicationReviewFeedbackEditModel.Name =
                    await orchestrator.GetApplicationReviewFeedbackViewModelAsync(applicationReviewFeedbackEditModel);
                return View(applicationReviewFeedbackEditModel);
            }

            TempData[TempDateArModel] = JsonConvert.SerializeObject(applicationReviewFeedbackEditModel);
            return RedirectToRoute(RouteNames.ApplicationReviewConfirmation_Get,
                new
                {
                    applicationReviewFeedbackEditModel.ApplicationReviewId,
                    applicationReviewFeedbackEditModel.VacancyId, applicationReviewFeedbackEditModel.Ukprn
                });
        }

        [HttpGet("status", Name = RouteNames.ApplicationReviewConfirmation_Get)]
        public async Task<IActionResult> ApplicationStatusConfirmation(
            ApplicationReviewRouteModel applicationReviewEditModel)
        {
            if (TempData[TempDateArModel] is string model)
            {
                var applicationReviewEditViewModel = JsonConvert.DeserializeObject<ApplicationReviewEditModel>(model);
                var applicationStatusConfirmationViewModel =
                    await orchestrator.GetApplicationStatusConfirmationViewModelAsync(applicationReviewEditViewModel);
                return View(applicationStatusConfirmationViewModel);
            }

            return RedirectToRoute(RouteNames.ApplicationReview_Get,
                new
                {
                    applicationReviewEditModel.ApplicationReviewId, applicationReviewEditModel.VacancyId,
                    applicationReviewEditModel.Ukprn
                });
        }

        [HttpPost("status", Name = RouteNames.ApplicationReviewConfirmation_Post)]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        public async Task<IActionResult> ApplicationStatusConfirmation(
            ApplicationReviewStatusConfirmationEditModel editModel)
        {
            if (!ModelState.IsValid)
            {
                var vm = await orchestrator.GetApplicationStatusConfirmationViewModelAsync(editModel);
                return View(vm);
            }

            if (editModel.CanNotifyCandidate)
            {
                var statusChangeInfo =
                    await orchestrator.PostApplicationReviewStatusChangeModelAsync(editModel, User.ToVacancyUser());

                if (statusChangeInfo.ShouldMakeOthersUnsuccessful)
                {
                    TempData.Add(TempDataKeys.ApplicationReviewStatusInfoMessage,
                        InfoMessages.ApplicationReviewSingleSuccessStatusHeader);
                    return RedirectToRoute(RouteNames.ApplicationReviewsToUnsuccessful_Get,
                        new {editModel.VacancyId, editModel.Ukprn});
                }

                var isAllApplicationsHasOutcome =
                    await orchestrator.IsAllApplicationReviewsHasOutcomeAsync(editModel.VacancyId);
                if (isAllApplicationsHasOutcome)
                {
                    TempData.TryAdd(TempDataKeys.ArchiveVacancyInfoMessage,
                        InfoMessages.VacancyApplicantsOutcomeNotified);
                    return RedirectToRoute(RouteNames.ArchiveVacancy_Get, new {editModel.VacancyId, editModel.Ukprn});
                }

                switch (editModel.Outcome)
                {
                    case ApplicationReviewStatus.Successful:
                        TempData.Add(TempDataKeys.ApplicationReviewSuccessStatusInfoMessage,
                            InfoMessages.ApplicationReviewSingleSuccessStatusHeader);
                        break;
                    case ApplicationReviewStatus.Unsuccessful:
                        TempData.Add(TempDataKeys.ApplicationReviewUnsuccessStatusInfoMessage,
                            InfoMessages.ApplicationEmployerUnsuccessfulHeader);
                        break;
                    default:
                        TempData.Add(TempDataKeys.ApplicationReviewStatusInfoMessage,
                            string.Format(InfoMessages.ApplicationReviewStatusHeader, statusChangeInfo.CandidateName,
                                editModel.Outcome?.ToString().ToLower()));
                        break;
                }
            }

            return RedirectToRoute(RouteNames.VacancyManage_Get, new {editModel.VacancyId, editModel.Ukprn});
        }
    }
}