using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.EditVacancyDates;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyManage;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Employer.Web.QueryStringModels;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class EditVacancyDatesController : Controller
    {
        private readonly EditVacancyDatesOrchestrator _orchestrator;

        public EditVacancyDatesController(EditVacancyDatesOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("edit-dates", Name = RouteNames.VacancyEditDates_Get)]
        public async Task<IActionResult> EditVacancyDates(VacancyRouteModel vrm, [FromQuery]string proposedClosingDate, [FromQuery]string proposedStartDate)
        {
            var vacancy = await _orchestrator.GetVacancyAsync(vrm);

            if (vacancy.CanEdit)
            {
                return HandleRedirectOfDraftVacancy(vacancy);
            }

            if (vacancy.Status != VacancyStatus.Live)
            {
                return RedirectToRoute(RouteNames.DisplayVacancy_Get);
            }

            var response = await _orchestrator.GetEditVacancyDatesViewModel(vrm, proposedClosingDate, proposedStartDate);

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            return View(response.Data);
        }

        [HttpPost("edit-dates", Name = RouteNames.VacancyEditDates_Post)]
        public async Task<IActionResult> EditVacancyDates(EditVacancyDatesEditModel m)
        {
            var vacancy = await _orchestrator.GetVacancyAsync(m);

            if (vacancy.CanEdit)
            {
                return HandleRedirectOfDraftVacancy(vacancy);
            }

            if (vacancy.Status != VacancyStatus.Live)
            {
                return RedirectToRoute(RouteNames.DisplayVacancy_Get);
            }

            var response = await _orchestrator.ValidateEditVacancyDatesViewModel(m);

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetEditVacancyDatesViewModel(m);
                return View(vm);
            }

            var proposedChangesViewModel = new ProposedChangesViewModel
            {
                VacancyId = m.VacancyId,
                EmployerAccountId = m.EmployerAccountId,
                ProposedClosingDate = m.ClosingDate.ToDateQueryString(),
                ProposedStartDate = m.StartDate.ToDateQueryString()
            };

            return RedirectToRoute(RouteNames.VacancyManage_Get, proposedChangesViewModel);
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