using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.AboutEmployer;
using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class AboutEmployerController(AboutEmployerOrchestrator orchestrator) : Controller
{
    [HttpGet("about-employer", Name = RouteNames.AboutEmployer_Get)]
    public async Task<IActionResult> AboutEmployer(TaskListViewModel model)
    {
        var vm = await orchestrator.GetAboutEmployerViewModelAsync(model);
        return View(vm);
    }

    [HttpPost("about-employer", Name =  RouteNames.AboutEmployer_Post)]
    public async Task<IActionResult> AboutEmployer(AboutEmployerEditModel m)
    {
        var response = await orchestrator.PostAboutEmployerEditModelAsync(m, User.ToVacancyUser());

        if (!response.Success)
        {
            response.AddErrorsToModelState(ModelState);
        }
        var vm = await orchestrator.GetAboutEmployerViewModelAsync(m);
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        return vm.IsTaskListCompleted
            ? RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new { m.VacancyId, m.EmployerAccountId })
            : RedirectToRoute(RouteNames.EmployerContactDetails_Get, new { m.VacancyId, m.EmployerAccountId, wizard = m.IsTaskList });
    }
}