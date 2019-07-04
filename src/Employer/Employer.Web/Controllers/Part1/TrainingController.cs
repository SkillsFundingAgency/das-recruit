using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Training;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class TrainingController : Controller
    {
        private readonly TrainingOrchestrator _orchestrator;

        public TrainingController(TrainingOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }
        
        [HttpGet("training", Name = RouteNames.Training_Get)]
        public async Task<IActionResult> Training(VacancyRouteModel vrm, [FromQuery] string wizard = "true", [FromQuery] string clear = "", [FromQuery] string hasTraining = "")
        {
            var vm = await _orchestrator.GetTrainingViewModelAsync(vrm, User.ToVacancyUser());

            if (vm.IsUsersFirstVacancy && string.IsNullOrEmpty(hasTraining))
                return RedirectToRoute(RouteNames.Training_First_Time_Get);

            vm.PageInfo.SetWizard(wizard);

            if (string.IsNullOrWhiteSpace(clear) == false)
            {
                vm.SelectedProgrammeId = "";
            }

            return View(vm);
        }

        [HttpPost("training", Name = RouteNames.Training_Post)]
        public async Task<IActionResult> Training(TrainingEditModel m, [FromQuery] bool wizard)
        {
            var user = User.ToVacancyUser();
            var response = await _orchestrator.PostTrainingEditModelAsync(m, user);
            
            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetTrainingViewModelAsync(m, user);
                vm.PageInfo.SetWizard(wizard);

                return View(vm);
            }

            return wizard
                ? RedirectToRoute(RouteNames.TrainingProvider_Select_Get)
                : RedirectToRoute(RouteNames.Vacancy_Preview_Get);
        }

        [HttpGet("training-first-vacancy", Name = RouteNames.Training_First_Time_Get)]
        public async Task<IActionResult> TrainingFirstVacancy(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetTrainingFirstVacancyViewModelAsync(vrm);

            return View(vm);
        }

        [HttpPost("training-first-vacancy", Name = RouteNames.Training_First_Time_Post)]
        public async Task<IActionResult> TrainingFirstVacancy(TrainingFirstVacancyEditModel m)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetTrainingFirstVacancyViewModelAsync(m);
                return View(vm);
            }

            if (m.HasFoundTraining.Value)
                return RedirectToRoute(RouteNames.Training_Get, new {hasTraining = true});

            return RedirectToRoute(RouteNames.Training_Help_Get);
        }

        [HttpGet("training-help", Name = RouteNames.Training_Help_Get)]
        public IActionResult TrainingHelp(VacancyRouteModel vrm)
        {
            return View();
        }
    }
}