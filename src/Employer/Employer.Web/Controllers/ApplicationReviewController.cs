using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Shared.Web.ViewModels;
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
            return View(vm);
        }

        [HttpPost("", Name = RouteNames.ApplicationReview_Post)]
        public async Task<IActionResult> ApplicationReview(ApplicationReviewEditModel editModel, [FromQuery] bool vacancySharedByProvider)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetApplicationReviewViewModelAsync(editModel, vacancySharedByProvider);
                return View(vm);
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
                var applicationReviewStatusEditModel = new ApplicationReviewStatusConfirmationEditModel 
                {
                    ApplicationReviewId = editModel.ApplicationReviewId,
                    VacancyId = editModel.VacancyId,
                    EmployerAccountId = editModel.EmployerAccountId,
                    CandidateFeedback = editModel.CandidateFeedback,
                    Outcome = editModel.Outcome,
                };
                var candidateName = await _orchestrator.PostApplicationReviewConfirmationEditModelAsync(applicationReviewStatusEditModel, User.ToVacancyUser());
                TempData.Add(TempDataKeys.ApplicationReviewStatusChangeInfoMessage, string.Format(InfoMessages.ApplicationStatusChangeBannerHeader, candidateName, editModel.Outcome.ToString().ToLower()));
                return RedirectToRoute(RouteNames.VacancyManage_Get, new { editModel.VacancyId, editModel.EmployerAccountId });
            }

            TempData[TempDataARModel] = JsonConvert.SerializeObject(editModel);
            return RedirectToRoute(RouteNames.ApplicationReviewConfirmation_Get, new { editModel.VacancyId, editModel.EmployerAccountId, editModel.ApplicationReviewId });
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
                var candidateName = await _orchestrator.PostApplicationReviewConfirmationEditModelAsync(editModel, User.ToVacancyUser());
                TempData.Add(TempDataKeys.ApplicationReviewStatusInfoMessage, string.Format(InfoMessages.ApplicationReviewStatusHeader, candidateName, editModel.Outcome.ToString().ToLower()));
                return RedirectToRoute(RouteNames.VacancyManage_Get, new { editModel.VacancyId, editModel.EmployerAccountId });
            }
            return RedirectToRoute(RouteNames.ApplicationReview_Get, new { editModel.VacancyId, editModel.EmployerAccountId, editModel.ApplicationReviewId });
        }
    }
}
