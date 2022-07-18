using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.WorkExperience;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    [Authorize(Policy = nameof(PolicyNames.IsTraineeshipWeb))]
    public class WorkExperienceController : Controller
    {
        private readonly WorkExperienceOrchestrator _orchestrator;

        public WorkExperienceController(WorkExperienceOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }
        
        [HttpGet("work-experience", Name = RouteNames.WorkExperience_Get)]
        public async Task<IActionResult> WorkExperience(VacancyRouteModel vacancyRouteModel)
        {
            var viewModel = await _orchestrator.GetWorkExperienceViewModelAsync(vacancyRouteModel);
            return View(viewModel);
        }

        [HttpPost("work-experience", Name = RouteNames.WorkExperience_Post)]
        public async Task<IActionResult> WorkExperience(WorkExperienceEditModel editModel)
        {
            var response = await _orchestrator.PostWorkExperienceEditModelAsync(editModel, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }
            
            var vm = await _orchestrator.GetWorkExperienceViewModelAsync(editModel);
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            
            if (!vm.IsTaskListCompleted)
            {
                return RedirectToRoute(RouteNames.Duration_Get, new {editModel.VacancyId, editModel.Ukprn});
            }
            return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new {editModel.VacancyId, editModel.Ukprn});
        }
    }
}