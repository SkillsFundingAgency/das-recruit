using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class VacancyViewController : Controller
    {
        private readonly VacancyViewOrchestrator _orchestrator;
        private readonly IUtility _utility;
        private readonly IFeature _feature;

        public VacancyViewController(VacancyViewOrchestrator orchestrator, IUtility utility, IFeature feature)
        {
            _orchestrator = orchestrator;
            _utility = utility;
            _feature = feature;
        }

        [HttpGet("", Name = RouteNames.DisplayVacancy_Get)]
        public async Task<IActionResult> DisplayVacancy(VacancyRouteModel vrm)
        {
            var vacancy = await _orchestrator.GetVacancy(vrm);

            if (vacancy.CanEdit)
            {
                return HandleRedirectOfEditableVacancy(vacancy);
            }

            var m = await _orchestrator.GetVacancyDisplayViewModelAsync(vacancy);
            return View(m.ViewName, m.ViewModel);
        }

        [HttpGet("view", Name = RouteNames.DisplayFullVacancy_Get)]
        public async Task<IActionResult> DisplayFullVacancy(VacancyRouteModel vrm)
        {
            var vacancy = await _orchestrator.GetVacancy(vrm);

            if (vacancy.CanEdit)
            {
                return HandleRedirectOfEditableVacancy(vacancy);
            }

            var vm = await _orchestrator.GetFullVacancyDisplayViewModelAsync(vacancy);
            return View(ViewNames.FullVacancyView, vm);
        }

        private IActionResult HandleRedirectOfEditableVacancy(Vacancy vacancy)
        {
            if (_feature.IsFeatureEnabled(FeatureNames.ProviderTaskList))
            {
                return RedirectToRoute(RouteNames.ProviderTaskListGet, new {vacancy.TrainingProvider.Ukprn, vacancyId = vacancy.Id});
            }
            
            if (_utility.VacancyHasCompletedPartOne(vacancy))
            {
                return RedirectToRoute(RouteNames.Vacancy_Preview_Get, new {vacancy.TrainingProvider.Ukprn, vacancyId = vacancy.Id});
            }

            var resumeRouteName = _utility.GetPermittedRoutesForVacancy(vacancy).Last();

            return RedirectToRoute(resumeRouteName, new { wizard = "true",vacancy.TrainingProvider.Ukprn, vacancyId = vacancy.Id });
        }
    }
}
