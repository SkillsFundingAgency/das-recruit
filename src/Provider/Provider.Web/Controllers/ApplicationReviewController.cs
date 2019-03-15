using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Shared.Web.ViewModels;
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
        public async Task<IActionResult> ApplicationReview(ApplicationReviewEditModel applicationReviewEditModel)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetApplicationReviewViewModelAsync(applicationReviewEditModel);
                return View(vm);
            }
            TempData[TempDateARModel] = JsonConvert.SerializeObject(applicationReviewEditModel);
            return RedirectToRoute(RouteNames.ApplicationReviewConfirmation_Get);
        }

        [HttpGet("status", Name = RouteNames.ApplicationReviewConfirmation_Get)]
        public async Task<IActionResult> ApplicationStatusConfirmation()
        {
            if (TempData[TempDateARModel] is string model)
            {
                var applicationReviewEditViewModel = JsonConvert.DeserializeObject<ApplicationReviewEditModel>(model);
                var applicationStatusConfirmationViewModel = await _orchestrator.GetApplicationStatusConfirmationViewModelAsync(applicationReviewEditViewModel);
                return View(applicationStatusConfirmationViewModel);
            }
            return RedirectToRoute(RouteNames.ApplicationReview_Get);
        }

        [HttpPost("status", Name = RouteNames.ApplicationReviewConfirmation_Post)]
        public async Task<IActionResult> ApplicationStatusConfirmation(ApplicationReviewStatusConfirmationEditModel applicationReviewStatusConfirmationEditModel)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetApplicationStatusConfirmationViewModelAsync(applicationReviewStatusConfirmationEditModel);
                return View(vm);
            }

            if (applicationReviewStatusConfirmationEditModel.CanNotifyCandidate)
            {
                var candidateName = await _orchestrator.PostApplicationReviewConfirmationEditModelAsync(applicationReviewStatusConfirmationEditModel, User.ToVacancyUser());
                TempData.Add(TempDataKeys.ApplicationReviewStatusInfoMessage, string.Format(InfoMessages.ApplicationReviewStatusHeader, candidateName, applicationReviewStatusConfirmationEditModel.Outcome.ToString().ToLower()));
                return RedirectToRoute(RouteNames.VacancyManage_Get);
            }
            return RedirectToRoute(RouteNames.ApplicationReview_Get);
        }
    }
}