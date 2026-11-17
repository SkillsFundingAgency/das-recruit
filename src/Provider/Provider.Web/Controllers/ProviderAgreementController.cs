using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class ProviderAgreementController(ProviderAgreementOrchestrator orchestrator) : Controller
{
    [HttpGet("provider-agreement", Name = RouteNames.ProviderAgreement_HardStop_Get)]
    public async Task<IActionResult> ProviderAgreementHardStop(VacancyRouteModel vrm)
    {
        var hasAgreement = await orchestrator.HasAgreementAsync(vrm.Ukprn);
        return hasAgreement
            ? RedirectToRoute(RouteNames.Vacancy_Advert_Preview_Get, new { vrm.Ukprn, vrm.VacancyId })
            : View(vrm); 
    }
}