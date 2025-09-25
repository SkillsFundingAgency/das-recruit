using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Training;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1;

[Route(RoutePaths.AccountVacancyRoutePath)]
[Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
public class TrainingController(TrainingOrchestrator orchestrator) : Controller
{
    private const string InvalidTraining = "Enter a valid training course";

    [HttpGet("training", Name = RouteNames.Training_Get)]
    public async Task<IActionResult> Training(VacancyRouteModel vrm, [FromQuery] string wizard = "true", [FromQuery] string clear = "", [FromQuery] string programmeId = "")
    {
        var clearTraining = string.IsNullOrWhiteSpace(clear) == false;

        var vm = await orchestrator.GetTrainingViewModelAsync(vrm, User.ToVacancyUser());

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
        if (string.IsNullOrWhiteSpace(m.SelectedProgrammeId))
            return await ProgrammeNotFound(m, wizard);

        var programme = await orchestrator.GetProgrammeAsync(m.SelectedProgrammeId);
        if (programme is null)
            return await ProgrammeNotFound(m, wizard);

        if (!ModelState.IsValid)
        {
            var vm = await orchestrator.GetTrainingViewModelAsync(m, User.ToVacancyUser());
            vm.PageInfo.SetWizard(wizard);

            return View(vm);
        }

        return RedirectToRoute(RouteNames.Training_Confirm_Get, new { programmeId = m.SelectedProgrammeId, wizard, m.Ukprn, m.VacancyId });
    }

    [HttpGet("training-confirm", Name = RouteNames.Training_Confirm_Get)]
    public async Task<IActionResult> ConfirmTraining(VacancyRouteModel vrm, string programmeId, [FromQuery] bool wizard)
    {
        var vm = await orchestrator.GetConfirmTrainingViewModelAsync(vrm, programmeId);

        if (vm == null)
        {
            var m = new TrainingEditModel { SelectedProgrammeId = programmeId, Ukprn = vrm.Ukprn, VacancyId = vrm.VacancyId };
            return await ProgrammeNotFound(m, wizard);
        }

        vm.PageInfo.SetWizard(wizard);

        return View(vm);
    }

    [HttpPost("training-confirm", Name = RouteNames.Training_Confirm_Post)]
    public async Task<IActionResult> ConfirmTraining(
        [FromServices] IRecruitVacancyClient vacancyClient,
        [FromServices] ITaskListValidator taskListValidator, 
        ConfirmTrainingEditModel m,
        [FromQuery] bool wizard)
    {
        var user = User.ToVacancyUser();
        var response = await orchestrator.PostConfirmTrainingEditModelAsync(m, user);
        if (!response.Success)
        {
            response.AddErrorsToModelState(ModelState);
        }
            
        if (!ModelState.IsValid)
        {
            var vm = await orchestrator.GetTrainingViewModelAsync(m, user);
            vm.PageInfo.SetWizard(wizard);
            return View("training", vm);
        }

        var vacancy = await vacancyClient.GetVacancyAsync(m.VacancyId!.Value);
        bool isTaskListComplete = await taskListValidator.IsCompleteAsync(vacancy, ProviderTaskListSectionFlags.All);

        return isTaskListComplete switch
        {
            true => RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new { m.VacancyId, m.Ukprn }),
            _ => RedirectToRoute(RouteNames.ShortDescription_Get, new { m.VacancyId, m.Ukprn })
        };
    }

    private async Task<IActionResult> ProgrammeNotFound(TrainingEditModel m, bool wizard)
    {
        ModelState.AddModelError(nameof(TrainingEditModel.SelectedProgrammeId), InvalidTraining);

        var vm = await orchestrator.GetTrainingViewModelAsync(m, User.ToVacancyUser());
        vm.PageInfo.SetWizard(wizard);
        return View(ViewNames.Training, vm);
    }
}