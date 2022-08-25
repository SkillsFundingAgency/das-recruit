using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers.Part1;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class LegalEntityAgreementController : EmployerControllerBase
    {
        private readonly LegalEntityAgreementOrchestrator _orchestrator;
        private readonly IFeature _feature;

        public LegalEntityAgreementController(
            LegalEntityAgreementOrchestrator orchestrator, IWebHostEnvironment hostingEnvironment, IFeature feature)
            : base(hostingEnvironment)
        {
            _orchestrator = orchestrator;
            _feature = feature;
        }

        [HttpGet("legal-entity-agreement", Name = RouteNames.LegalEntityAgreement_SoftStop_Get)]
        public async Task<IActionResult> LegalEntityAgreementSoftStop(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var info = GetVacancyEmployerInfoCookie(vrm.VacancyId);
            var vm = await _orchestrator.GetLegalEntityAgreementSoftStopViewModelAsync(vrm, info.AccountLegalEntityPublicHashedId);
            vm.PageInfo.SetWizard(wizard);

            if (vm.HasLegalEntityAgreement == false)
                return View(vm);

            if (_feature.IsFeatureEnabled(FeatureNames.EmployerTaskList))
            {
                if (vm.PageInfo.HasCompletedPartOne)
                {
                    return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet);
                }
                return RedirectToRoute(RouteNames.AboutEmployer_Get, new { Wizard = wizard });
            }
            
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
