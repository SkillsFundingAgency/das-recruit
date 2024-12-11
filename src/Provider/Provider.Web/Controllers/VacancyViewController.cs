using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class VacancyViewController : Controller
    {
        private readonly VacancyViewOrchestrator _orchestrator;
        private readonly IUtility _utility;

        public VacancyViewController(VacancyViewOrchestrator orchestrator, IUtility utility)
        {
            _orchestrator = orchestrator;
            _utility = utility;
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
            vm.Ukprn = vrm.Ukprn;
            vm.VacancyId = vrm.VacancyId;
            return View(ViewNames.FullVacancyView, vm);
        }

        private IActionResult HandleRedirectOfEditableVacancy(Vacancy vacancy)
        {
            if (_utility.IsTaskListCompleted(vacancy))
            {
                return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new {vacancy.TrainingProvider.Ukprn, vacancyId = vacancy.Id});
            }
            return RedirectToRoute(RouteNames.ProviderTaskListGet, new {vacancy.TrainingProvider.Ukprn, vacancyId = vacancy.Id});
            
        }
    }
}
