using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.VacancyDescription;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Microsoft.AspNetCore.Authorization;

namespace Esfa.Recruit.Provider.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class VacancyDescriptionController : Controller
    {
        private readonly VacancyDescriptionOrchestrator _orchestrator;
        private readonly IFeature _feature;

        public VacancyDescriptionController(VacancyDescriptionOrchestrator orchestrator, IFeature feature)
        {
            _orchestrator = orchestrator;
            _feature = feature;
        }

        [HttpGet("vacancy-description", Name = RouteNames.VacancyDescription_Index_Get)]
        public async Task<IActionResult> VacancyDescription(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetVacancyDescriptionViewModelAsync(vrm);
            return View(vm);
        }

        [HttpPost("vacancy-description", Name =  RouteNames.VacancyDescription_Index_Post)]
        public async Task<IActionResult> VacancyDescription(VacancyDescriptionEditModel m)
        {
            var response = await _orchestrator.PostVacancyDescriptionEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            var vm = await _orchestrator.GetVacancyDescriptionViewModelAsync(m);
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            if (_feature.IsFeatureEnabled(FeatureNames.ProviderTaskList))
            {
                if (!vm.IsTaskListCompleted)
                {
                    return RedirectToRoute(RouteNames.ProviderTaskListGet, new {m.Ukprn, m.VacancyId});
                }
                return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new {m.Ukprn, m.VacancyId});
                
            }
            
            return RedirectToRoute(RouteNames.Vacancy_Preview_Get, new {m.Ukprn, m.VacancyId});
        }
    }
}