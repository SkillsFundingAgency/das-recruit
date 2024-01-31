using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Training;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class TrainingController : Controller
    {
        private readonly TrainingOrchestrator _orchestrator;
        private readonly IFeature _feature;
        private const string InvalidTraining = "Select the training the apprentice will take";

        public TrainingController(TrainingOrchestrator orchestrator, IFeature feature)
        {
            _orchestrator = orchestrator;
            _feature = feature;
        }
        
        [HttpGet("training", Name = RouteNames.Training_Get)]
        public async Task<IActionResult> Training(VacancyRouteModel vrm, [FromQuery] string wizard = "true", [FromQuery] string clear = "", [FromQuery] string hasTraining = "", [FromQuery] string programmeId = "")
        {
            var clearTraining = string.IsNullOrWhiteSpace(clear) == false;

            var vm = await _orchestrator.GetTrainingViewModelAsync(vrm, User.ToVacancyUser());

            if (string.IsNullOrWhiteSpace(programmeId) == false &&
                vm.Programmes.Any(p => p.Id == programmeId))
            {
                vm.SelectedProgrammeId = programmeId;
            }

            var userHasFoundTraining = string.IsNullOrEmpty(hasTraining) == false || 
                                       clearTraining || 
                                       string.IsNullOrEmpty(vm.SelectedProgrammeId) == false;

            if (vm.IsUsersFirstVacancy &&
                userHasFoundTraining == false)
                return RedirectToRoute(RouteNames.Training_First_Time_Get, new {vrm.VacancyId, vrm.EmployerAccountId});

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

            return RedirectToRoute(RouteNames.Training_Confirm_Get, new {programmeId = m.SelectedProgrammeId, wizard, m.VacancyId, m.EmployerAccountId});
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
                return RedirectToRoute(RouteNames.Training_Get, new {hasTraining = true, m.VacancyId, m.EmployerAccountId});

            return RedirectToRoute(RouteNames.Training_Help_Get, new {m.VacancyId, m.EmployerAccountId});
        }

        [HttpGet("training-help", Name = RouteNames.Training_Help_Get)]
        public IActionResult TrainingHelp(VacancyRouteModel vrm)
        {
            return View(vrm);
        }

        [HttpGet("training-confirm", Name = RouteNames.Training_Confirm_Get)]
        public async Task<IActionResult> ConfirmTraining(VacancyRouteModel vrm, string programmeId, [FromQuery] bool wizard)
        {
            var vm = await _orchestrator.GetConfirmTrainingViewModelAsync(vrm, programmeId);

            if (vm == null)
            {
                var m = new TrainingEditModel { SelectedProgrammeId = programmeId, EmployerAccountId = vrm.EmployerAccountId, VacancyId = vrm.VacancyId};
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

            if(programme == null)
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
                ? RedirectToRoute(RouteNames.TrainingProvider_Select_Get, new {m.VacancyId, m.EmployerAccountId})
                : RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new {m.VacancyId, m.EmployerAccountId});            
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