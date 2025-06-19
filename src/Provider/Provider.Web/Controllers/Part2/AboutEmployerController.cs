using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.AboutEmployer;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part2;

[Route(RoutePaths.AccountVacancyRoutePath)]
[Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
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
            ? RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new { m.Ukprn, m.VacancyId })
            : RedirectToRoute(RouteNames.ProviderContactDetails_Get, new { m.Ukprn, m.VacancyId, wizard = m.IsTaskList });
    }
}