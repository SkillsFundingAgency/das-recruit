using Esfa.Recruit.Shared.Web.FeatureToggle;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.CustomWage;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class CustomWageController : Controller
    {
        private readonly ICustomWageOrchestrator _orchestrator;
        private readonly IFeature _feature;

        public CustomWageController(ICustomWageOrchestrator orchestrator, IFeature feature)
        {
            _orchestrator = orchestrator;
            _feature = feature;
        }

        [HttpGet("custom-wage", Name = RouteNames.CustomWage_Get)]
        public async Task<IActionResult> CustomWage(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var vm = await _orchestrator.GetCustomWageViewModelAsync(vrm);
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost("custom-wage", Name = RouteNames.CustomWage_Post)]
        public async Task<IActionResult> CustomWage(CustomWageEditModel m, [FromQuery] bool wizard)
        {
            var response = await _orchestrator.PostCustomWageEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetCustomWageViewModelAsync(m);
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }

            return RedirectToRoute(RouteNames.AddExtraInformation_Get, new { m.VacancyId, m.Ukprn, wizard });
        }
    }
}
