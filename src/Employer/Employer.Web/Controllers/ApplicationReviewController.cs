using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountApplicationReviewRoutePath)]
    public class ApplicationReviewController : Controller
    {
        private readonly ApplicationReviewOrchestrator _orchestrator;

        public ApplicationReviewController(ApplicationReviewOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("review", Name = RouteNames.ApplicationReview_Get)]
        public async Task<IActionResult> ApplicationReview(ApplicationReviewRouteModel rm)
        {
            var vm = await _orchestrator.GetApplicationReviewViewModelAsync(rm);
            return View(vm);
        }

        [HttpPost("review", Name = RouteNames.ApplicationReview_Post)]
        public async Task<IActionResult> ApplicationReview(ApplicationReviewEditModel m)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetApplicationReviewViewModelAsync(m);
                return View(vm);
            }            
            var applicationStatusConfirmationViewModel = await _orchestrator.GetApplicationStatusConfirmationViewModelAsync(m);
            return View("ApplicationStatusConfirmation", applicationStatusConfirmationViewModel);
        }

       [HttpPost("status", Name = RouteNames.ApplicationStatusConfirmation_Post)]
        public async Task<IActionResult> ApplicationStatusConfirmation(ApplicationReviewStatusConfirmationEditModel applicationReviewStatusConfirmationEditModel)
        {            
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetApplicationStatusConfirmationViewModelAsync(applicationReviewStatusConfirmationEditModel);
                return View(vm);
            }

            if (applicationReviewStatusConfirmationEditModel.NotifyApplicant.HasValue &&
                applicationReviewStatusConfirmationEditModel.NotifyApplicant.Value)
            {
                await _orchestrator.PostApplicationReviewEditModelAsync(applicationReviewStatusConfirmationEditModel, User.ToVacancyUser());
                var routeModel=new ApplicationReviewRouteModel {
                    ApplicationReviewId = applicationReviewStatusConfirmationEditModel.ApplicationReviewId,
                    VacancyId = applicationReviewStatusConfirmationEditModel.VacancyId,
                    EmployerAccountId = applicationReviewStatusConfirmationEditModel.EmployerAccountId
                };
                var vm = await _orchestrator.GetApplicationReviewViewModelAsync(routeModel);
                return RedirectToRoute(RouteNames.VacancyManage_Get, new { applicationUserName =vm.Name, applicationReviewStatus = applicationReviewStatusConfirmationEditModel.Outcome.ToString(), setApplicationStatus ="true"});
            }
           return RedirectToRoute(RouteNames.ApplicationReview_Get);
        }        
    }
}