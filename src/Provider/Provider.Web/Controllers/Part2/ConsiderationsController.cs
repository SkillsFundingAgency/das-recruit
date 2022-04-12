using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.Considerations;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class ConsiderationsController : Controller
    {
        private readonly ConsiderationsOrchestrator _orchestrator;
        private readonly IFeature _feature;

        public ConsiderationsController(ConsiderationsOrchestrator orchestrator, IFeature feature)
        {
            _orchestrator = orchestrator;
            _feature = feature;
        }

        [HttpGet("considerations", Name = RouteNames.Considerations_Get)]
        public async Task<IActionResult> Considerations(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetConsiderationsViewModelAsync(vrm);
            return View(vm);
        }

        [HttpPost("considerations", Name =  RouteNames.Considerations_Post)]
        public async Task<IActionResult> Considerations(ConsiderationsEditModel m)
        {
            var response = await _orchestrator.PostConsiderationsEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            var vm = await _orchestrator.GetConsiderationsViewModelAsync(m);
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            
            if (_feature.IsFeatureEnabled(FeatureNames.ProviderTaskList))
            {
                if (!vm.IsTaskListCompleted)
                {
                    return RedirectToRoute(RouteNames.ProviderTaskListGet, new {m.VacancyId, m.Ukprn});
                }
                return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new {m.VacancyId, m.Ukprn});
            }

            return RedirectToRoute(RouteNames.Vacancy_Preview_Get, new {m.VacancyId, m.Ukprn});
        }
    }
}