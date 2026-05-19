using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountApplicationReviewRoutePath)]
    public class ApplicationReviewController(IApplicationReviewOrchestrator orchestrator) : Controller
    {
        private const string TempDataArModel = "ApplicationReviewEditModel";

        [HttpGet("", Name = RouteNames.ApplicationReview_Get)]
        public async Task<IActionResult> ApplicationReview(ApplicationReviewRouteModel rm)
        {
            var vm = await orchestrator.GetApplicationReviewViewModelAsync(rm);
            var viewName = vm.IsFaaV2Application ? "ApplicationReviewV2" : "ApplicationReview";
            return View(viewName, vm);
        }

        [HttpPost("", Name = RouteNames.ApplicationReview_Post)]
        public async Task<IActionResult> ApplicationReview(ApplicationReviewEditModel editModel)
        {
            if (!ModelState.IsValid)
            {
                var vm = await orchestrator.GetApplicationReviewViewModelAsync(editModel);
                var viewName = vm.IsFaaV2Application ? "ApplicationReviewV2" : "ApplicationReview";
                return View(viewName, vm);
            }

            if (editModel.IsApplicationSharedByProvider)
            {
                var candidateInfo = await orchestrator.PostApplicationReviewEditModelAsync(editModel, User.ToVacancyUser());
                TempData.Add(TempDataKeys.ApplicationReviewStatusInfoMessage,
                    editModel.Outcome == ApplicationReviewStatus.EmployerInterviewing
                        ? string.Format(InfoMessages.ApplicationEmployerInterviewingHeader, candidateInfo.FriendlyId, candidateInfo.Name)
                        : InfoMessages.ApplicationEmployerUnsuccessfulHeader);

                TempData.Add(TempDataKeys.ApplicationReviewedInfoMessage,
                    editModel.Outcome == ApplicationReviewStatus.EmployerInterviewing
                        ? InfoMessages.ApplicationEmployerInterviewingBody
                        : InfoMessages.ApplicationEmployerUnsuccessfulBody);

                return RedirectToRoute( RouteNames.VacancyManage_Get, new {editModel.VacancyId, editModel.EmployerAccountId});
            }

            if (editModel.Outcome is ApplicationReviewStatus.InReview or ApplicationReviewStatus.Interviewing)
            {
                var confirmationEditModel = new ApplicationReviewStatusConfirmationEditModel 
                {
                    ApplicationReviewId = editModel.ApplicationReviewId,
                    VacancyId = editModel.VacancyId,
                    EmployerAccountId = editModel.EmployerAccountId,
                    CandidateFeedback = editModel.CandidateFeedback,
                    Outcome = editModel.Outcome,
                    NotifyCandidate = false
                };

                var statusInfo = await orchestrator.PostApplicationReviewConfirmationEditModelAsync(confirmationEditModel, User.ToVacancyUser());
                TempData.Add(TempDataKeys.ApplicationReviewStatusChangeInfoMessage, string.Format(InfoMessages.ApplicationStatusChangeBannerHeader, statusInfo.CandidateName, editModel.Outcome.GetDisplayName().ToLower()));
                return RedirectToRoute(RouteNames.VacancyManage_Get, new { editModel.VacancyId, editModel.EmployerAccountId });
            }

            if (editModel.Outcome is ApplicationReviewStatus.Unsuccessful)
            {
                TempData[TempDataArModel] = JsonConvert.SerializeObject(editModel);
                return RedirectToRoute(RouteNames.ApplicationReviewFeedback_Get,
                    new
                    {
                        editModel.ApplicationReviewId,
                        editModel.VacancyId,
                        editModel.EmployerAccountId
                    });
            }

            TempData[TempDataArModel] = JsonConvert.SerializeObject(editModel);
            return RedirectToRoute(RouteNames.ApplicationReviewConfirmation_Get, new { editModel.VacancyId, editModel.EmployerAccountId, editModel.ApplicationReviewId, editModel.Outcome, editModel.CandidateFeedback });
        }

        [HttpGet("feedback", Name = RouteNames.ApplicationReviewFeedback_Get)]
        public async Task<IActionResult> ApplicationFeedback(ApplicationReviewRouteModel applicationReviewEditModel)
        {
            if (TempData[TempDataArModel] is string model)
            {
                var applicationReviewEditViewModel = JsonConvert.DeserializeObject<ApplicationReviewEditModel>(model);
                var applicationReviewFeedbackViewModel = await orchestrator.GetApplicationReviewFeedbackViewModelAsync(applicationReviewEditViewModel);
                return View(applicationReviewFeedbackViewModel);
            }

            return RedirectToRoute(RouteNames.ApplicationReview_Get,
                new
                {
                    applicationReviewEditModel.ApplicationReviewId,
                    applicationReviewEditModel.VacancyId,
                    applicationReviewEditModel.EmployerAccountId
                });
        }

        [HttpPost("feedback", Name = RouteNames.ApplicationReviewFeedback_Post)]
        public async Task<IActionResult> ApplicationFeedback(ApplicationReviewFeedbackViewModel applicationReviewFeedbackEditModel)
        {
            if (!ModelState.IsValid)
            {
                var applicationReviewFeedbackViewModel = await orchestrator.GetApplicationReviewFeedbackViewModelAsync(applicationReviewFeedbackEditModel);
                applicationReviewFeedbackEditModel.Name = applicationReviewFeedbackViewModel.GetValueOrDefault("Name");
                applicationReviewFeedbackEditModel.FriendlyId = applicationReviewFeedbackViewModel.GetValueOrDefault("FriendlyId");
                return View(applicationReviewFeedbackEditModel);
            }

            TempData[TempDataArModel] = JsonConvert.SerializeObject(applicationReviewFeedbackEditModel);
            return RedirectToRoute(RouteNames.ApplicationReviewConfirmation_Get,
                new
                {
                    applicationReviewFeedbackEditModel.ApplicationReviewId,
                    applicationReviewFeedbackEditModel.VacancyId,
                    applicationReviewFeedbackEditModel.EmployerAccountId
                });
        }

        [HttpGet("status", Name = RouteNames.ApplicationReviewConfirmation_Get)]
        public async Task<IActionResult> ApplicationStatusConfirmation(ApplicationReviewEditModel editModel)
        {
            ModelState.Clear();

            if (TempData[TempDataArModel] is string model)
            {
                var applicationReviewEditViewModel = JsonConvert.DeserializeObject<ApplicationReviewEditModel>(model);
                var applicationStatusConfirmationViewModel = await orchestrator.GetApplicationStatusConfirmationViewModelAsync(applicationReviewEditViewModel);
                return View(applicationStatusConfirmationViewModel);
            }
            return RedirectToRoute(RouteNames.ApplicationReview_Get, new { editModel.VacancyId, editModel.EmployerAccountId, editModel.ApplicationReviewId });
        }

        [HttpPost("status", Name = RouteNames.ApplicationReviewConfirmation_Post)]
        public async Task<IActionResult> ApplicationStatusConfirmation(ApplicationReviewStatusConfirmationEditModel editModel)
        {
            if (!ModelState.IsValid)
            {
                var vm = await orchestrator.GetApplicationStatusConfirmationViewModelAsync(editModel);
                return View(vm);
            }

            if (editModel.CanNotifyCandidate)
            {
                var statusInfo = await orchestrator.PostApplicationReviewConfirmationEditModelAsync(editModel, User.ToVacancyUser());

                if (statusInfo.ShouldMakeOthersUnsuccessful)
                {
                    TempData.Add(TempDataKeys.ApplicationReviewStatusInfoMessage, InfoMessages.ApplicationReviewSingleSuccessStatusHeader);
                    return RedirectToRoute(RouteNames.ApplicationReviewsToUnsuccessful_Get, new { editModel.VacancyId, editModel.EmployerAccountId });
                }

                var isAllApplicationsHasOutcome = await orchestrator.IsAllApplicationReviewsHasOutcomeAsync(editModel.VacancyId);
                if (isAllApplicationsHasOutcome)
                {
                    TempData.TryAdd(TempDataKeys.ArchiveAdvertInfoMessage, InfoMessages.AdvertApplicantsOutcomeNotified);
                    return RedirectToRoute(RouteNames.ArchiveVacancy_Get, new { editModel.VacancyId, editModel.EmployerAccountId, });
                }

                TempData.Add(TempDataKeys.ApplicationReviewStatusChangeInfoMessage, string.Format(InfoMessages.ApplicationReviewStatusHeader, statusInfo.CandidateName, editModel.Outcome?.ToString().ToLower()));
                return RedirectToRoute(RouteNames.VacancyManage_Get, new {editModel.VacancyId, editModel.EmployerAccountId });
            }
            return RedirectToRoute(RouteNames.ApplicationReview_Get, new { editModel.VacancyId, editModel.EmployerAccountId, editModel.ApplicationReviewId });
        }
    }
}