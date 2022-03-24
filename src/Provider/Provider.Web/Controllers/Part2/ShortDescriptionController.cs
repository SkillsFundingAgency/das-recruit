using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.ShortDescription;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class ShortDescriptionController : Controller
    {
        private readonly ShortDescriptionOrchestrator _orchestrator;
        private readonly IFeature _feature;

        public ShortDescriptionController(ShortDescriptionOrchestrator orchestrator, IFeature feature)
        {
            _orchestrator = orchestrator;
            _feature = feature;
        }
        
        [HttpGet("description", Name = RouteNames.ShortDescription_Get)]
        public async Task<IActionResult> ShortDescription(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetShortDescriptionViewModelAsync(vrm);
            return View(vm);
        }

        [HttpPost("description", Name = RouteNames.ShortDescription_Post)]
        public async Task<IActionResult> ShortDescription(ShortDescriptionEditModel m)
        {
            var response = await _orchestrator.PostShortDescriptionEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetShortDescriptionViewModelAsync(m);
                return View(vm);
            }

            if (_feature.IsFeatureEnabled(FeatureNames.ProviderTaskList))
            {
                return RedirectToRoute(RouteNames.VacancyDescription_Index_Get, new {m.Ukprn, m.VacancyId});
            }
            return RedirectToRoute(RouteNames.Vacancy_Preview_Get, new {m.Ukprn, m.VacancyId});
        }
    }
}