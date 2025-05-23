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
    public class ApplicationReviewController : Controller
    {
        private readonly IApplicationReviewOrchestrator _orchestrator;
        private const string TempDateARModel = "ApplicationReviewEditModel";

        public ApplicationReviewController(IApplicationReviewOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("", Name = RouteNames.ApplicationReview_Get)]
        public async Task<IActionResult> ApplicationReview(ApplicationReviewRouteModel rm)
        {
            var vm = await _orchestrator.GetApplicationReviewViewModelAsync(rm);
            var viewName = vm.IsFaaV2Application ? "ApplicationReviewV2" : "ApplicationReview";
            return View(viewName, vm);
        }

        [HttpPost("", Name = RouteNames.ApplicationReview_Post)]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        public async Task<IActionResult> ApplicationReview(ApplicationReviewEditModel applicationReviewEditModel)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetApplicationReviewViewModelAsync(applicationReviewEditModel);
                var viewName = vm.IsFaaV2Application ? "ApplicationReviewV2" : "ApplicationReview";
                return View(viewName, vm);
            }

            switch (applicationReviewEditModel.Outcome.Value)
            {
                case ApplicationReviewStatus.Shared:
                    var shareApplicationsModel = new ShareApplicationReviewsRequest
                    {
                        Ukprn = applicationReviewEditModel.Ukprn,
                        VacancyId = applicationReviewEditModel.VacancyId,
                        ApplicationsToShare = new List<Guid>
                        {
                            applicationReviewEditModel.ApplicationReviewId
                        }
                    };
                    return RedirectToRoute(RouteNames.ApplicationReviewsToShareConfirmation_Get, shareApplicationsModel);

                case ApplicationReviewStatus.InReview:
                case ApplicationReviewStatus.Interviewing:
                    var statusChangeInfo = await _orchestrator.PostApplicationReviewStatusChangeModelAsync(applicationReviewEditModel, User.ToVacancyUser());
                    TempData.Add(TempDataKeys.ApplicationStatusChangedHeader, string.Format(InfoMessages.ApplicationStatusChangeBannerHeader, statusChangeInfo.CandidateName, applicationReviewEditModel.Outcome.GetDisplayName().ToLower()));
                    return RedirectToRoute(RouteNames.VacancyManage_Get, new { applicationReviewEditModel.VacancyId, applicationReviewEditModel.Ukprn });

                case ApplicationReviewStatus.Successful:
                    TempData[TempDateARModel] = JsonConvert.SerializeObject(applicationReviewEditModel);
                    return RedirectToRoute(RouteNames.ApplicationReviewConfirmation_Get, new { applicationReviewEditModel.ApplicationReviewId, applicationReviewEditModel.VacancyId, applicationReviewEditModel.Ukprn });

                case ApplicationReviewStatus.EmployerUnsuccessful:
                    applicationReviewEditModel.Outcome = ApplicationReviewStatus.Unsuccessful;
                    TempData[TempDateARModel] = JsonConvert.SerializeObject(applicationReviewEditModel);
                    return RedirectToRoute(RouteNames.ApplicationReviewConfirmation_Get, new { applicationReviewEditModel.ApplicationReviewId, applicationReviewEditModel.VacancyId, applicationReviewEditModel.Ukprn });
                
                case ApplicationReviewStatus.Unsuccessful:
                    TempData[TempDateARModel] = JsonConvert.SerializeObject(applicationReviewEditModel);
                    return RedirectToRoute(RouteNames.ApplicationReviewFeedback_Get, new { applicationReviewEditModel.ApplicationReviewId, applicationReviewEditModel.VacancyId, applicationReviewEditModel.Ukprn });

                default:
                    var vm = await _orchestrator.GetApplicationReviewViewModelAsync(applicationReviewEditModel);
                    return View(vm);
            }
        }

        [HttpGet("feedback", Name = RouteNames.ApplicationReviewFeedback_Get)]
        public async Task<IActionResult> ApplicationFeedback(ApplicationReviewRouteModel applicationReviewEditModel)
        {
            if (TempData[TempDateARModel] is string model)
            {
                var applicationReviewEditViewModel = JsonConvert.DeserializeObject<ApplicationReviewEditModel>(model);
                var applicationReviewFeedbackViewModel = await _orchestrator.GetApplicationReviewFeedbackViewModelAsync(applicationReviewEditViewModel);
                return View(applicationReviewFeedbackViewModel);
            }
            return RedirectToRoute(RouteNames.ApplicationReview_Get, new { applicationReviewEditModel.ApplicationReviewId, applicationReviewEditModel.VacancyId, applicationReviewEditModel.Ukprn });
        }

        [HttpPost("feedback", Name = RouteNames.ApplicationReviewFeedback_Post)]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        public async Task<IActionResult> ApplicationFeedback(ApplicationReviewFeedbackViewModel applicationReviewFeedbackEditModel)
        {
            if (!ModelState.IsValid)
            {
                applicationReviewFeedbackEditModel.Name = await _orchestrator.GetApplicationReviewFeedbackViewModelAsync(applicationReviewFeedbackEditModel);
                return View(applicationReviewFeedbackEditModel);
            }

            TempData[TempDateARModel] = JsonConvert.SerializeObject(applicationReviewFeedbackEditModel);
            return RedirectToRoute(RouteNames.ApplicationReviewConfirmation_Get, new { applicationReviewFeedbackEditModel.ApplicationReviewId, applicationReviewFeedbackEditModel.VacancyId, applicationReviewFeedbackEditModel.Ukprn });
        }
        [HttpGet("status", Name = RouteNames.ApplicationReviewConfirmation_Get)]
        public async Task<IActionResult> ApplicationStatusConfirmation(ApplicationReviewRouteModel applicationReviewEditModel)
        {
            if (TempData[TempDateARModel] is string model)
            {
                var applicationReviewEditViewModel = JsonConvert.DeserializeObject<ApplicationReviewEditModel>(model);
                var applicationStatusConfirmationViewModel = await _orchestrator.GetApplicationStatusConfirmationViewModelAsync(applicationReviewEditViewModel);
                return View(applicationStatusConfirmationViewModel);
            }
            return RedirectToRoute(RouteNames.ApplicationReview_Get, new { applicationReviewEditModel.ApplicationReviewId, applicationReviewEditModel.VacancyId, applicationReviewEditModel.Ukprn });
        }

        [HttpPost("status", Name = RouteNames.ApplicationReviewConfirmation_Post)]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        public async Task<IActionResult> ApplicationStatusConfirmation(ApplicationReviewStatusConfirmationEditModel editModel)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetApplicationStatusConfirmationViewModelAsync(editModel);
                return View(vm);
            }

            if (editModel.CanNotifyCandidate)
            {
                var statusChangeInfo = await _orchestrator.PostApplicationReviewStatusChangeModelAsync(editModel, User.ToVacancyUser());

                if (statusChangeInfo.ShouldMakeOthersUnsuccessful)
                {
                    TempData.Add(TempDataKeys.ApplicationReviewStatusInfoMessage, string.Format(InfoMessages.ApplicationReviewSingleSuccessStatusHeader, statusChangeInfo.CandidateName));
                    return RedirectToRoute(RouteNames.ApplicationReviewsToUnsuccessful_Get, new { editModel.VacancyId, editModel.Ukprn });
                }

                switch (editModel.Outcome)
                {
                    case ApplicationReviewStatus.Successful:
                        TempData.Add(TempDataKeys.ApplicationReviewSuccessStatusInfoMessage, string.Format(InfoMessages.ApplicationReviewSingleSuccessStatusHeader, statusChangeInfo.CandidateName));
                        break;
                    case ApplicationReviewStatus.Unsuccessful:
                        TempData.Add(TempDataKeys.ApplicationReviewUnsuccessStatusInfoMessage, string.Format(InfoMessages.ApplicationEmployerUnsuccessfulHeader, statusChangeInfo.CandidateName));
                        break;
                    default:
                        TempData.Add(TempDataKeys.ApplicationReviewStatusInfoMessage, string.Format(InfoMessages.ApplicationReviewStatusHeader, statusChangeInfo.CandidateName, editModel.Outcome.ToString().ToLower()));
                        break;
                }
            }
            return RedirectToRoute(RouteNames.VacancyManage_Get, new { editModel.VacancyId, editModel.Ukprn });
        }
    }
}