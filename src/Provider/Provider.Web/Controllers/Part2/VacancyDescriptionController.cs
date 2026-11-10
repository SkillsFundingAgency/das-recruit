using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.VacancyDescription;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace Esfa.Recruit.Provider.Web.Controllers.Part2;

[Route(RoutePaths.AccountVacancyRoutePath)]
[Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
public class VacancyDescriptionController(VacancyDescriptionOrchestrator orchestrator) : Controller
{
    [HttpGet("vacancy-description", Name = RouteNames.VacancyDescription_Index_Get)]
    public async Task<IActionResult> VacancyDescription(VacancyRouteModel vrm)
    {
        var vm = await orchestrator.GetVacancyDescriptionViewModelAsync(vrm);
        return View(vm);
    }

    [HttpPost("vacancy-description", Name =  RouteNames.VacancyDescription_Index_Post)]
    public async Task<IActionResult> VacancyDescription(VacancyDescriptionEditModel m)
    {
        var response = await orchestrator.PostVacancyDescriptionEditModelAsync(m, User.ToVacancyUser());

        if (!response.Success)
        {
            response.AddErrorsToModelState(ModelState);
        }

        var vm = await orchestrator.GetVacancyDescriptionViewModelAsync(m);
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        return vm.IsTaskListCompleted
            ? RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new { m.Ukprn, m.VacancyId })
            : RedirectToRoute(RouteNames.ProviderTaskListGet, new { m.Ukprn, m.VacancyId });
    }
}