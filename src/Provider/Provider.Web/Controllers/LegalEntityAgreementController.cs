using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class LegalEntityAgreementController(LegalEntityAgreementOrchestrator orchestrator) : Controller
{
    [HttpGet("legal-entity-agreement-stop", Name = RouteNames.LegalEntityAgreement_HardStop_Get)]
    public async Task<IActionResult> LegalEntityAgreementHardStop(VacancyRouteModel vrm)
    {
        var vm = await orchestrator.GetLegalEntityAgreementHardStopViewModelAsync(vrm);

        if (vm.HasLegalEntityAgreement == false)
            return View(vm);

        return RedirectToRoute(RouteNames.Dashboard_Get, new {vrm.Ukprn});
    }
}