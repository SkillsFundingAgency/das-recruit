using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers.Part1;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class LegalEntityAgreementController(LegalEntityAgreementOrchestrator orchestrator, IWebHostEnvironment hostingEnvironment)
    : EmployerControllerBase(hostingEnvironment)
{
    [HttpGet("legal-entity-agreement", Name = RouteNames.LegalEntityAgreement_SoftStop_Get)]
    public async Task<IActionResult> LegalEntityAgreementSoftStop(TaskListViewModel vrm)
    {
        var info = GetVacancyEmployerInfoCookie(vrm.VacancyId);
        var vm = await orchestrator.GetLegalEntityAgreementSoftStopViewModelAsync(vrm, info.AccountLegalEntityPublicHashedId);

        if (vm.HasLegalEntityAgreement == false)
            return View(vm);

        return vm.IsTaskListComplete
            ? RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new {vrm.VacancyId, vrm.EmployerAccountId})
            : RedirectToRoute(RouteNames.AboutEmployer_Get, new { vrm.VacancyId, vrm.EmployerAccountId, wizard = vrm.IsTaskList });
    }

    [HttpGet("legal-entity-agreement-stop", Name = RouteNames.LegalEntityAgreement_HardStop_Get)]
    public async Task<IActionResult> LegalEntityAgreementHardStop(TaskListViewModel model)
    {
        var vm = await orchestrator.GetLegalEntityAgreementHardStopViewModelAsync(model);

        return vm.HasLegalEntityAgreement
            ? RedirectToRoute(RouteNames.Dashboard_Get, new {model.EmployerAccountId})
            : View(vm);
    }
}