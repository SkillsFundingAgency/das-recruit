using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Views;
using Esfa.Recruit.Shared.Web.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class LegalEntityAgreementController : Controller
    {
        private readonly LegalEntityAgreementOrchestrator _orchestrator;

        public LegalEntityAgreementController(LegalEntityAgreementOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("legal-entity-agreement", Name = RouteNames.LegalEntityAgreement_SoftStop_Get)]
        public async Task<IActionResult> LegalEntityAgreementSoftStop(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var vm = await _orchestrator.GetLegalEntityAgreementSoftStopViewModelAsync(vrm);
            vm.PageInfo.SetWizard(wizard);

            if (vm.HasLegalEntityAgreement == false)
                return View(vm);

            return vm.PageInfo.IsWizard
                    ? RedirectToRoute(RouteNames.Location_Get)
                    : RedirectToRoute(RouteNames.Vacancy_Preview_Get, null, Anchors.AboutEmployerSection);
        }

        [HttpGet("legal-entity-agreement-stop", Name = RouteNames.LegalEntityAgreement_HardStop_Get)]
        public async Task<IActionResult> LegalEntityAgreementHardStop(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetLegalEntityAgreementHardStopViewModelAsync(vrm);

            if(vm.HasLegalEntityAgreement == false)
                return View(vm);

            return RedirectToRoute(RouteNames.Dashboard_Index_Get);
        }
    }
}
