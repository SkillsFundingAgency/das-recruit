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

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class WageController : Controller
    {
        private readonly WageOrchestrator _orchestrator;
        private readonly IFeature _feature;

        public WageController(WageOrchestrator orchestrator, IFeature feature)
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
            
            if (wizard)
            {
                return RedirectToRoute(RouteNames.NumberOfPositions_Get, new {m.VacancyId, m.EmployerAccountId, wizard});    
            }

        [FeatureGate(FeatureNames.CompetitiveSalary)]
        [HttpGet("competitive-wage", Name = RouteNames.SetCompetitivePayRate_Get)]
        public async Task<IActionResult> CompetitiveSalary(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var vm = await _orchestrator.GetCompetitiveWageViewModelAsync(vrm);
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }
    }
}