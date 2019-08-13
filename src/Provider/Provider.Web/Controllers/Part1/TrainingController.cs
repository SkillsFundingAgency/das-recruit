using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Training;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1
{    
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class TrainingController : Controller
    {
        private readonly TrainingOrchestrator _orchestrator;
        private const string InvalidTraining = "Please select a training programme";

        public TrainingController(TrainingOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("training", Name = RouteNames.Training_Get)]
        public async Task<IActionResult> Training(VacancyRouteModel vrm, [FromQuery] string wizard = "true", [FromQuery] string clear = "", [FromQuery] string programmeId = "")
        {
            var clearTraining = string.IsNullOrWhiteSpace(clear) == false;

            var vm = await _orchestrator.GetTrainingViewModelAsync(vrm, User.ToVacancyUser());

            if (string.IsNullOrWhiteSpace(programmeId) == false &&
                vm.Programmes.Any(p => p.Id == programmeId))
            {
                vm.SelectedProgrammeId = programmeId;
            }

            if (clearTraining)
                vm.SelectedProgrammeId = "";

            vm.PageInfo.SetWizard(wizard);

            return View(vm);
        }

        [HttpPost("training", Name = RouteNames.Training_Post)]
        public async Task<IActionResult> Training(TrainingEditModel m, [FromQuery] bool wizard)
        {
            var programme = await _orchestrator.GetProgrammeAsync(m.SelectedProgrammeId);

            if (programme == null)
            {
                return await ProgrammeNotFound(m, wizard);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetTrainingViewModelAsync(m, User.ToVacancyUser());
                vm.PageInfo.SetWizard(wizard);

                return View(vm);
            }

            return RedirectToRoute(RouteNames.Training_Confirm_Get, new { programmeId = m.SelectedProgrammeId, wizard });
        }

        [HttpGet("training-confirm", Name = RouteNames.Training_Confirm_Get)]
        public async Task<IActionResult> ConfirmTraining(VacancyRouteModel vrm, string programmeId, [FromQuery] bool wizard)
        {
            var vm = await _orchestrator.GetConfirmTrainingViewModelAsync(vrm, programmeId);

            if (vm == null)
            {
                var m = new TrainingEditModel { SelectedProgrammeId = programmeId, Ukprn = vrm.Ukprn, VacancyId = vrm.VacancyId };
                return await ProgrammeNotFound(m, wizard);
            }

            vm.PageInfo.SetWizard(wizard);

            return View(vm);
        }

        [HttpPost("training-confirm", Name = RouteNames.Training_Confirm_Post)]
        public async Task<IActionResult> ConfirmTraining(ConfirmTrainingEditModel m, [FromQuery] bool wizard)
        {
            var user = User.ToVacancyUser();

            var programme = await _orchestrator.GetProgrammeAsync(m.ProgrammeId);

            if (programme == null)
            {
                ModelState.AddModelError(nameof(TrainingEditModel.SelectedProgrammeId), InvalidTraining);
            }
            else
            {
                var response = await _orchestrator.PostConfirmTrainingEditModelAsync(m, user);

                if (!response.Success)
                {
                    response.AddErrorsToModelState(ModelState);
                }
            }
            
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetTrainingViewModelAsync(m, user);
                vm.PageInfo.SetWizard(wizard);

                return View("training", vm);
            }

            return wizard
                ? RedirectToRoute(RouteNames.NumberOfPositions_Get)
                : RedirectToRoute(RouteNames.Vacancy_Preview_Get);
        }

        private async Task<IActionResult> ProgrammeNotFound(TrainingEditModel m, bool wizard)
        {
            ModelState.AddModelError(nameof(TrainingEditModel.SelectedProgrammeId), InvalidTraining);

            var vm = await _orchestrator.GetTrainingViewModelAsync(m, User.ToVacancyUser());
            vm.PageInfo.SetWizard(wizard);
            return View(ViewNames.Training, vm);
        }
    }
}