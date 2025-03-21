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
    public class ApplicationReviewController : Controller
    {
        private readonly IApplicationReviewOrchestrator _orchestrator;
        private const string TempDataARModel = "ApplicationReviewEditModel";

        public ApplicationReviewController(IApplicationReviewOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("", Name = RouteNames.ApplicationReview_Get)]
        public async Task<IActionResult> ApplicationReview(ApplicationReviewRouteModel rm, [FromQuery] bool vacancySharedByProvider)
        {
            var vm = await _orchestrator.GetApplicationReviewViewModelAsync(rm, vacancySharedByProvider);
            var viewName = vm.IsFaaV2Application ? "ApplicationReviewV2" : "ApplicationReview";
            return View(viewName, vm);
        }

        [HttpPost("", Name = RouteNames.ApplicationReview_Post)]
        public async Task<IActionResult> ApplicationReview(ApplicationReviewEditModel editModel, [FromQuery] bool vacancySharedByProvider)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetApplicationReviewViewModelAsync(editModel, vacancySharedByProvider);
                var viewName = vm.IsFaaV2Application ? "ApplicationReviewV2" : "ApplicationReview";
                return View(viewName, vm);
            }

            if (vacancySharedByProvider)
            {
                var candidateInfo = await _orchestrator.PostApplicationReviewEditModelAsync(editModel, User.ToVacancyUser(), vacancySharedByProvider);
                TempData.Add(TempDataKeys.ApplicationReviewStatusInfoMessage,
                    editModel.Outcome == ApplicationReviewStatus.EmployerInterviewing
                        ? string.Format(InfoMessages.ApplicationEmployerInterviewingHeader, candidateInfo.FriendlyId, candidateInfo.Name)
                        : string.Format(InfoMessages.ApplicationEmployerUnsuccessfulHeader, candidateInfo.FriendlyId));

                TempData.Add(TempDataKeys.ApplicationReviewedInfoMessage,
                    editModel.Outcome == ApplicationReviewStatus.EmployerInterviewing
                        ? string.Format(InfoMessages.ApplicationEmployerInterviewingBody)
                        : string.Format(InfoMessages.ApplicationEmployerUnsuccessfulBody));

                return RedirectToRoute( RouteNames.VacancyManage_Get, new { editModel.EmployerAccountId, editModel.VacancyId, vacancySharedByProvider });
            }

            if (editModel.Outcome == ApplicationReviewStatus.InReview || editModel.Outcome == ApplicationReviewStatus.Interviewing)
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

                var statusInfo = await _orchestrator.PostApplicationReviewConfirmationEditModelAsync(confirmationEditModel, User.ToVacancyUser());
                TempData.Add(TempDataKeys.ApplicationReviewStatusChangeInfoMessage, string.Format(InfoMessages.ApplicationStatusChangeBannerHeader, statusInfo.CandidateName, editModel.Outcome.GetDisplayName().ToLower()));
                return RedirectToRoute(RouteNames.VacancyManage_Get, new { editModel.VacancyId, editModel.EmployerAccountId });
            }

            TempData[TempDataARModel] = JsonConvert.SerializeObject(editModel);
            return RedirectToRoute(RouteNames.ApplicationReviewConfirmation_Get, new { editModel.VacancyId, editModel.EmployerAccountId, editModel.ApplicationReviewId, editModel.Outcome, editModel.CandidateFeedback });
        }

        [HttpGet("status", Name = RouteNames.ApplicationReviewConfirmation_Get)]
        public async Task<IActionResult> ApplicationStatusConfirmation(ApplicationReviewEditModel editModel)
        {
            if (TempData[TempDataARModel] is string model)
            {
                var applicationReviewEditViewModel = JsonConvert.DeserializeObject<ApplicationReviewEditModel>(model);
                var applicationStatusConfirmationViewModel = await _orchestrator.GetApplicationStatusConfirmationViewModelAsync(applicationReviewEditViewModel);
                return View(applicationStatusConfirmationViewModel);
            }
            return RedirectToRoute(RouteNames.ApplicationReview_Get, new { editModel.VacancyId, editModel.EmployerAccountId, editModel.ApplicationReviewId });
        }

        [HttpPost("status", Name = RouteNames.ApplicationReviewConfirmation_Post)]
        public async Task<IActionResult> ApplicationStatusConfirmation(ApplicationReviewStatusConfirmationEditModel editModel)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetApplicationStatusConfirmationViewModelAsync(editModel);
                return View(vm);
            }

            if (editModel.CanNotifyCandidate)
            {
                var statusInfo = await _orchestrator.PostApplicationReviewConfirmationEditModelAsync(editModel, User.ToVacancyUser());

                if (statusInfo.ShouldMakeOthersUnsuccessful) 
                {
                    TempData.Add(TempDataKeys.ApplicationReviewStatusInfoMessage, string.Format(InfoMessages.ApplicationReviewSingleSuccessStatusHeader, statusInfo.CandidateName));
                    return RedirectToRoute(RouteNames.ApplicationReviewsToUnsuccessful_Get, new { editModel.VacancyId, editModel.EmployerAccountId });
                }

                TempData.Add(TempDataKeys.ApplicationReviewStatusChangeInfoMessage, string.Format(InfoMessages.ApplicationReviewStatusHeader, statusInfo.CandidateName, editModel.Outcome.ToString().ToLower()));
                return RedirectToRoute(RouteNames.VacancyManage_Get, new { editModel.VacancyId, editModel.EmployerAccountId });
            }
            return RedirectToRoute(RouteNames.ApplicationReview_Get, new { editModel.VacancyId, editModel.EmployerAccountId, editModel.ApplicationReviewId });
        }
    }
}
