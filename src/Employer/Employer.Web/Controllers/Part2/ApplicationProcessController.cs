using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.ApplicationProcess;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class ApplicationProcessController(ApplicationProcessOrchestrator orchestrator) : Controller
{
    [HttpGet("application-process", Name = RouteNames.ApplicationProcess_Get)]
    public async Task<IActionResult> ApplicationProcess(TaskListViewModel vrm)
    {
        var vm = await orchestrator.GetApplicationProcessViewModelAsync(vrm);
        return View(vm);
    }

    [HttpPost("application-process", Name = RouteNames.ApplicationProcess_Post)]
    public async Task<IActionResult> ApplicationProcess(ApplicationProcessEditModel m)
    {   
        var response = await orchestrator.PostApplicationProcessEditModelAsync(m, User.ToVacancyUser());
        if (!response.Success)
        {
            response.AddErrorsToModelState(ModelState);
        }
            
        var vm = await orchestrator.GetApplicationProcessViewModelAsync(m);
        if (!ModelState.IsValid)
        {
            return View(vm);
        }
            
        if (vm.IsTaskListCompleted)
        {
            return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new {m.VacancyId, m.EmployerAccountId});
        }
            
        return RedirectToRoute(RouteNames.EmployerTaskListGet, new {m.VacancyId, m.EmployerAccountId});
    }
}