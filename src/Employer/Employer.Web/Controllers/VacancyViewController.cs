﻿using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class VacancyViewController : Controller
    {
        private readonly VacancyViewOrchestrator _orchestrator;

        public VacancyViewController(VacancyViewOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
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
            if (Utility.VacancyHasCompletedPartOne(vacancy))
            {
                return RedirectToRoute(RouteNames.Vacancy_Preview_Get);
            }

            var resumeRouteName = Utility.GetValidRoutesForVacancy(vacancy).Last();

            return RedirectToRoute(resumeRouteName);
        }
    }
}
