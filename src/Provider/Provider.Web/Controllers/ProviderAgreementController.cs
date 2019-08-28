using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class ProviderAgreementController : Controller
    {
        public ProviderAgreementOrchestrator _orchestrator;

        public ProviderAgreementController(ProviderAgreementOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("provider-agreement", Name = RouteNames.ProviderAgreement_HardStop_Get)]
        public async Task<IActionResult> ProviderAgreementHardStop(VacancyRouteModel vrm)
        {
            var hasAgreement = await _orchestrator.HasAgreementAsync(vrm.Ukprn);

            if (hasAgreement)
                return RedirectToRoute(RouteNames.Vacancy_Preview_Get);

            return View();
        }
    }
}
