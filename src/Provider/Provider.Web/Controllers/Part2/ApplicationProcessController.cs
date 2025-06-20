﻿using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.ApplicationProcess;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part2;

[Route(RoutePaths.AccountVacancyRoutePath)]
[Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
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

        return vm.IsTaskListCompleted
            ? RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new { m.VacancyId, m.Ukprn })
            : RedirectToRoute(RouteNames.ProviderTaskListGet, new { m.VacancyId, m.Ukprn });
    }
}