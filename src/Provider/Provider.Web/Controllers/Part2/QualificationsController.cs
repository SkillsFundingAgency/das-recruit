using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.Qualifications;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.ViewModels.Qualifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part2;

[Route(RoutePaths.AccountVacancyRoutePath)]
[Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
public class QualificationsController(QualificationsOrchestrator orchestrator) : Controller
{
    private const string QualificationDeletedTempDataKey = "QualificationDeletedTempDataKey";

    [HttpGet("qualifications", Name = RouteNames.Qualifications_Get)]
    public async Task<IActionResult> Qualifications(VacancyRouteModel vrm)
    {
        var vm = await orchestrator.GetQualificationsViewModelAsync(vrm);

        if (TempData[QualificationDeletedTempDataKey] != null)
            vm.InfoMessage = "Successfully removed qualification";

        return View(vm);
    }
        
    [HttpPost("qualifications", Name = RouteNames.Qualifications_Get)]
    public async Task<IActionResult> Qualifications(AddQualificationsEditModel m)
    {
        if (!ModelState.IsValid)
        {
            var vm = await orchestrator.GetQualificationsViewModelAsync(m);
            return View(vm);
        }

        await orchestrator.PostAddQualificationEditModel(m, User.ToVacancyUser());

        if (m.AddQualificationRequirement is true)
        {
            return RedirectToRoute(RouteNames.Qualification_Add_Get, new { m.VacancyId, m.Ukprn });
        }

        return RedirectToRoute(m.IsTaskListCompleted
            ? RouteNames.ProviderCheckYourAnswersGet
            : RouteNames.FutureProspects_Get, new { m.VacancyId, m.Ukprn });
    }

    [HttpGet("qualifications/add", Name = RouteNames.Qualification_Add_Get)]
    public async Task<IActionResult> Qualification(VacancyRouteModel vrm)
    {
        var vm = await orchestrator.GetQualificationViewModelForAddAsync(vrm);

        return View(vm);
    }

    [HttpGet("qualifications/{index:int}", Name = RouteNames.Qualification_Edit_Get)]
    public async Task<IActionResult> Qualification(VacancyRouteModel vrm, int index)
    {
        var vm = await orchestrator.GetQualificationViewModelForEditAsync(vrm, index);
        return View(vm);
    }

    [HttpPost("qualifications/add", Name = RouteNames.Qualification_Add_Post)]
    public async Task<IActionResult> Qualification(VacancyRouteModel vrm, QualificationEditModel m)
    {
        var response = await orchestrator.PostQualificationEditModelForAddAsync(vrm, m, User.ToVacancyUser());

        if (!response.Success)
        {
            response.AddErrorsToModelState(ModelState);
        }

        if (!ModelState.IsValid)
        {
            var vm = await orchestrator.GetQualificationViewModelForAddAsync(vrm, m);

            return View(vm);
        }

        return RedirectToRoute(RouteNames.Qualifications_Get, new {vrm.VacancyId, vrm.Ukprn});
    }

    [HttpPost("qualifications/{index:int}", Name = RouteNames.Qualification_Edit_Post)]
    public async Task<IActionResult> Qualification(VacancyRouteModel vrm, QualificationEditModel m, [FromRoute] int index)
    {
        var response = await orchestrator.PostQualificationEditModelForEditAsync(vrm, m, User.ToVacancyUser(), index);

        if (!response.Success)
        {
            response.AddErrorsToModelState(ModelState);
        }

        if (!ModelState.IsValid)
        {
            var vm = await orchestrator.GetQualificationViewModelForEditAsync(vrm, m, index);

            return View(vm);
        }

        return RedirectToRoute(RouteNames.Qualifications_Get, new {vrm.VacancyId, vrm.Ukprn});
    }

    [HttpPost("qualifications/delete", Name = RouteNames.Qualification_Delete_Post)]
    public async Task<IActionResult> DeleteQualification(VacancyRouteModel vrm, [FromForm] int index)
    {
        await orchestrator.DeleteQualificationAsync(vrm, index, User.ToVacancyUser());

        TempData[QualificationDeletedTempDataKey] = 1;
        return RedirectToRoute(RouteNames.Qualifications_Get, new {vrm.VacancyId, vrm.Ukprn});
    }
}