using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Wage;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
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

            if (_feature.IsFeatureEnabled(FeatureNames.ProviderTaskList))
            {
                return RedirectToRoute(RouteNames.NumberOfPositions_Get, new { Wizard = wizard, m.VacancyId, m.Ukprn });
            }

            return wizard
                ? RedirectToRoute(RouteNames.Part1Complete_Get,new { m.VacancyId, m.Ukprn })
                : RedirectToRoute(RouteNames.Vacancy_Preview_Get, new { m.VacancyId, m.Ukprn });
        }
    }
}