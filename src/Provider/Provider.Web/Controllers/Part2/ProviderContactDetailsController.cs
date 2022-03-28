using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.ProviderContactDetails;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Microsoft.AspNetCore.Authorization;

namespace Esfa.Recruit.Provider.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class ProviderContactDetailsController : Controller
    {
        private readonly ProviderContactDetailsOrchestrator _orchestrator;
        private readonly IFeature _feature;

        public ProviderContactDetailsController(ProviderContactDetailsOrchestrator orchestrator, IFeature feature)
        {
            _orchestrator = orchestrator;
            _feature = feature;
        }

        [HttpGet("provider-contact-details", Name = RouteNames.ProviderContactDetails_Get)]
        public async Task<IActionResult> ProviderContactDetails(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetProviderContactDetailsViewModelAsync(vrm);
            return View(vm);
        }

        [HttpPost("provider-contact-details", Name = RouteNames.ProviderContactDetails_Post)]
        public async Task<IActionResult> ProviderContactDetails(ProviderContactDetailsEditModel m)
        {      
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetProviderContactDetailsViewModelAsync(m);
                return View(vm);
            }
            
            var response = await _orchestrator.PostProviderContactDetailsEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetProviderContactDetailsViewModelAsync(m);
                return View(vm);
            }
            
            if (_feature.IsFeatureEnabled(FeatureNames.ProviderTaskList))
            {
                return RedirectToRoute(RouteNames.ApplicationProcess_Get, new {m.VacancyId, m.Ukprn});
            }

            return RedirectToRoute(RouteNames.Vacancy_Preview_Get, new {m.VacancyId, m.Ukprn});
        }
    }
}