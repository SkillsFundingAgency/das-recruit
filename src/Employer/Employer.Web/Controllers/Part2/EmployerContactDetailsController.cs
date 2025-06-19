using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.EmployerContactDetails;
using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class EmployerContactDetailsController(EmployerContactDetailsOrchestrator orchestrator) : Controller
{
    [HttpGet("employer-contact-details", Name = RouteNames.EmployerContactDetails_Get)]
    public async Task<IActionResult> EmployerContactDetails(TaskListViewModel model)
    {
        var vm = await orchestrator.GetEmployerContactDetailsViewModelAsync(model);
        return View(vm);
    }

    [HttpPost("employer-contact-details", Name = RouteNames.EmployerContactDetails_Post)]
    public async Task<IActionResult> EmployerContactDetails(EmployerContactDetailsEditModel m)
    {            
        var response = await orchestrator.PostEmployerContactDetailsEditModelAsync(m, User.ToVacancyUser());
        if (!response.Success)
        {
            response.AddErrorsToModelState(ModelState);
        }

        var vm = await orchestrator.GetEmployerContactDetailsViewModelAsync(m);
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        return vm.IsTaskListCompleted
            ? RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new { m.VacancyId, m.EmployerAccountId })
            : RedirectToRoute(RouteNames.ApplicationProcess_Get, new { m.VacancyId, m.EmployerAccountId, wizard = m.IsTaskList }); 
    }
}