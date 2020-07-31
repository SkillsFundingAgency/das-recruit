using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers.Part1;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class LegalEntityAgreementController : EmployerControllerBase
    {
        private readonly LegalEntityAgreementOrchestrator _orchestrator;

        public LegalEntityAgreementController(
            LegalEntityAgreementOrchestrator orchestrator, IHostingEnvironment hostingEnvironment)
            : base(hostingEnvironment)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("legal-entity-agreement", Name = RouteNames.LegalEntityAgreement_SoftStop_Get)]
        public async Task<IActionResult> LegalEntityAgreementSoftStop(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var info = GetVacancyEmployerInfoCookie(vrm.VacancyId);
            var vm = await _orchestrator.GetLegalEntityAgreementSoftStopViewModelAsync(vrm, info.AccountLegalEntityPublicHashedId);
            vm.PageInfo.SetWizard(wizard);

            if (vm.HasLegalEntityAgreement == false)
                return View(vm);

            return RedirectToRoute(RouteNames.Location_Get, new {Wizard = wizard});
        }

        [HttpGet("legal-entity-agreement-stop", Name = RouteNames.LegalEntityAgreement_HardStop_Get)]
        public async Task<IActionResult> LegalEntityAgreementHardStop(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetLegalEntityAgreementHardStopViewModelAsync(vrm);

            if(vm.HasLegalEntityAgreement == false)
                return View(vm);

            return RedirectToRoute(RouteNames.Dashboard_Get);
        }
    }
}
