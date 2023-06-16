using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReview;
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
        private readonly ApplicationReviewOrchestrator _orchestrator;
        private const string TempDateARModel = "ApplicationReviewEditModel";

        public ApplicationReviewController(ApplicationReviewOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("", Name = RouteNames.ApplicationReview_Get)]
        public async Task<IActionResult> ApplicationReview(ApplicationReviewRouteModel rm)
        {
            var vm = await _orchestrator.GetApplicationReviewViewModelAsync(rm);
            return View(vm);
        }

        [HttpPost("", Name = RouteNames.ApplicationReview_Post)]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        public async Task<IActionResult> ApplicationReview(ApplicationReviewEditModel applicationReviewEditModel)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetApplicationReviewViewModelAsync(applicationReviewEditModel);
                return View(vm);
            }

            switch (applicationReviewEditModel.Outcome.Value)
            {
                case ApplicationReviewStatus.InReview:
                    var candidateName = await _orchestrator.PostApplicationReviewStatusChangeModelAsync(applicationReviewEditModel, User.ToVacancyUser());
                    TempData.Add(TempDataKeys.InReviewApplicationHeader, string.Format(InfoMessages.InReviewApplicationBannerHeader, candidateName));
                    return RedirectToRoute(RouteNames.VacancyManage_Get, new { applicationReviewEditModel.VacancyId, applicationReviewEditModel.Ukprn });

                case ApplicationReviewStatus.Successful: case ApplicationReviewStatus.Unsuccessful:
                    TempData[TempDateARModel] = JsonConvert.SerializeObject(applicationReviewEditModel);
                    return RedirectToRoute(RouteNames.ApplicationReviewConfirmation_Get, new { applicationReviewEditModel.ApplicationReviewId, applicationReviewEditModel.VacancyId, applicationReviewEditModel.Ukprn });

                default:
                    var vm = await _orchestrator.GetApplicationReviewViewModelAsync(applicationReviewEditModel);
                    return View(vm);
            }
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
            return RedirectToRoute(RouteNames.ApplicationReview_Get, new {applicationReviewEditModel.ApplicationReviewId, applicationReviewEditModel.VacancyId, applicationReviewEditModel.Ukprn});
        }

        [HttpPost("status", Name = RouteNames.ApplicationReviewConfirmation_Post)]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        public async Task<IActionResult> ApplicationStatusConfirmation(ApplicationReviewStatusConfirmationEditModel applicationReviewStatusConfirmationEditModel)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetApplicationStatusConfirmationViewModelAsync(applicationReviewStatusConfirmationEditModel);
                return View(vm);
            }

            if (applicationReviewStatusConfirmationEditModel.CanNotifyCandidate)
            {
                var candidateName = await _orchestrator.PostApplicationReviewStatusChangeModelAsync(applicationReviewStatusConfirmationEditModel, User.ToVacancyUser());
                TempData.Add(TempDataKeys.ApplicationReviewStatusInfoMessage, string.Format(InfoMessages.ApplicationReviewStatusHeader, candidateName, applicationReviewStatusConfirmationEditModel.Outcome.ToString().ToLower()));
                return RedirectToRoute(RouteNames.VacancyManage_Get, new {applicationReviewStatusConfirmationEditModel.VacancyId, applicationReviewStatusConfirmationEditModel.Ukprn});
            }
            return RedirectToRoute(RouteNames.ApplicationReview_Get, new {applicationReviewStatusConfirmationEditModel.ApplicationReviewId, applicationReviewStatusConfirmationEditModel.VacancyId, applicationReviewStatusConfirmationEditModel.Ukprn});
        }
    }
}