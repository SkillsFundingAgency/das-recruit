using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Microsoft.FeatureManagement.Mvc;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Employer.Web.RouteModel.Wage;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class WageController : Controller
    {
        private readonly IWageOrchestrator _orchestrator;
        private readonly IFeature _feature;

        public WageController(IWageOrchestrator orchestrator, IFeature feature)
        {
            _orchestrator = orchestrator;
            _feature = feature;
        }
        
        [HttpGet("wage", Name = RouteNames.Wage_Get)]
        public async Task<IActionResult> Wage(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var vm = await _orchestrator.GetWageViewModelAsync(vrm);
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost("wage", Name = RouteNames.Wage_Post)]
        public async Task<IActionResult> Wage(WageEditModel m, [FromQuery] bool wizard)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetWageViewModelAsync(m);
                return View(vm);
            }

            switch (m.WageType)
            {
                case WageType.FixedWage:
                    return RedirectToRoute(RouteNames.CustomWage_Get, new { m.VacancyId, m.EmployerAccountId, wizard });
                case WageType.NationalMinimumWage or WageType.NationalMinimumWageForApprentices:

                    var response = await _orchestrator.PostWageEditModelAsync(m, User.ToVacancyUser());

                    if (!response.Success)
                    {
                        response.AddErrorsToModelState(ModelState);
                    }

                    if (!ModelState.IsValid)
                    {
                        var vm = await _orchestrator.GetWageViewModelAsync(m);
                        vm.PageInfo.SetWizard(wizard);
                        return View(vm);
                    }

                    return RedirectToRoute(RouteNames.AddExtraInformation_Get, new { m.VacancyId, m.EmployerAccountId, wizard });

                case WageType.CompetitiveSalary:
                    return RedirectToRoute(RouteNames.SetCompetitivePayRate_Get, new { m.VacancyId, m.EmployerAccountId, wizard });
                default:
                    var resp = await _orchestrator.GetWageViewModelAsync(m);
                    return View(resp);
            }
        }

        [FeatureGate(FeatureNames.CompetitiveSalary)]
        [HttpGet("competitive-wage", Name = RouteNames.SetCompetitivePayRate_Get)]
        public async Task<IActionResult> CompetitiveSalary(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetCompetitiveWageViewModelAsync(vrm);
            return View(vm);
        }

        [FeatureGate(FeatureNames.CompetitiveSalary)]
        [HttpPost("competitive-wage", Name = RouteNames.SetCompetitivePayRate_Post)]
        public IActionResult CompetitiveSalary(CompetitiveWageEditModel m)
        {
            m.WageType = WageType.CompetitiveSalary;
            return RedirectToRoute(RouteNames.AddExtraInformation_Get, new { m.VacancyId, m.EmployerAccountId, m.WageType, m.CompetitiveSalaryType });
        }

        [HttpGet("extra-information-wage", Name = RouteNames.AddExtraInformation_Get)]
        public async Task<IActionResult> AdditionalInformation(WageExtraInformationRouteModel vrm, [FromQuery] string wizard = "true")
        {
            //var vm = await _orchestrator.GetExtraInformationViewModelAsync(vrm);
            //vm.PageInfo.SetWizard(wizard);
            return View();
        }
    }
}