using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Esfa.Recruit.Employer.Web.Configuration;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class VacancyManageController : Controller
    {
        private readonly VacancyManageOrchestrator _orchestrator;

        public VacancyManageController(VacancyManageOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("", Name = RouteNames.DisplayVacancy_Get)]
        public async Task<IActionResult> DisplayVacancy(VacancyRouteModel vrm)
        {
            var vacancy = await _orchestrator.GetVacancy(vrm);

            if (vacancy.CanEdit)
            {
                return HandleRedirectOfDraftVacancy(vacancy);
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
                return HandleRedirectOfDraftVacancy(vacancy);
            }

            if (vacancy.ApplicationMethod == ApplicationMethod.ThroughExternalApplicationSite || (vacancy.Status != VacancyStatus.Live && vacancy.Status != VacancyStatus.Closed))
            {
                return RedirectToRoute(RouteNames.DisplayVacancy_Get);
            }

            var vm = await _orchestrator.GetFullVacancyDisplayViewModelAsync(vacancy);
            return View(ViewNames.FullVacancyView, vm);
        }

        private IActionResult HandleRedirectOfDraftVacancy(Vacancy vacancy)
        {
            if (Utility.VacancyHasCompletedPartOne(vacancy))
            {
                return RedirectToRoute(RouteNames.Vacancy_Preview_Get);
            }

            var resumeRouteName = Utility.GetValidRoutesForVacancy(vacancy).Last();

            return RedirectToRoute(resumeRouteName);
        }
    }
}