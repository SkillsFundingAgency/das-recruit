using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Duration;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1;

[Route(RoutePaths.AccountVacancyRoutePath)]
[Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
public class DurationController(DurationOrchestrator orchestrator) : Controller
{
    [HttpGet("duration", Name = RouteNames.Duration_Get)]
    public async Task<IActionResult> Duration(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
    {
        var vm = await orchestrator.GetDurationViewModelAsync(vrm);
        vm.PageInfo.SetWizard(wizard);
        return View(vm);
    }

    [HttpPost("duration", Name = RouteNames.Duration_Post)]
    public async Task<IActionResult> Duration(DurationEditModel m, [FromQuery] bool wizard)
    {
        var response = await orchestrator.PostDurationEditModelAsync(m, User.ToVacancyUser());
            
        if (!response.Success)
        {
            response.AddErrorsToModelState(ModelState);
        }

        if (!ModelState.IsValid)
        {
            var vm = await orchestrator.GetDurationViewModelAsync(m);
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }
            
        return wizard
            ? RedirectToRoute(RouteNames.Wage_Get, new {m.Ukprn, m.VacancyId})
            : RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new {m.Ukprn, m.VacancyId});
    }
}