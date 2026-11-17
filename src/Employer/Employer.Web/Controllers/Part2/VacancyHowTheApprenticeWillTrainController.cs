using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.VacancyHowTheApprenticeWillTrain;
using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class VacancyHowTheApprenticeWillTrainController(VacancyHowWillTheApprenticeTrainOrchestrator orchestrator)
    : Controller
{
    [HttpGet("how-will-the-apprentice-train", Name = RouteNames.VacancyHowTheApprenticeWillTrain_Index_Get)]
    public async Task<IActionResult> VacancyHowTheApprenticeWillTrain(VacancyRouteModel vrm)
    {
        var vm = await orchestrator.GetVacancyDescriptionViewModelAsync(vrm);
        return View(vm);
    }

    [HttpPost("how-will-the-apprentice-train", Name =  RouteNames.VacancyHowTheApprenticeWillTrain_Index_Post)]
    public async Task<IActionResult> VacancyHowTheApprenticeWillTrain(VacancyHowTheApprenticeWillTrainEditModel m)
    {
        var response = await orchestrator.PostVacancyDescriptionEditModelAsync(m, User.ToVacancyUser());

        if (!response.Success)
        {
            response.AddErrorsToModelState(ModelState);
        }

        var vm = await orchestrator.GetVacancyDescriptionViewModelAsync(m);
        if (!ModelState.IsValid)
        {
            vm.AdditionalTrainingDescription = m.AdditionalTrainingDescription;
            vm.TrainingDescription = m.TrainingDescription;
            return View(vm);
        }
            
        if (vm.IsTaskListCompleted)
        {
            return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new {m.VacancyId, m.EmployerAccountId});
        }
        return RedirectToRoute(RouteNames.EmployerTaskListGet, new {m.VacancyId, m.EmployerAccountId});
    }
}