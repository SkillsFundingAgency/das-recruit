using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.EditVacancyDates;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.Configuration;
using Microsoft.AspNetCore.Hosting;
using System;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class EditVacancyDatesController : Controller
    {
        private readonly EditVacancyDatesOrchestrator _orchestrator;
        private readonly IHostingEnvironment _hostingEnvironment;

        public EditVacancyDatesController(EditVacancyDatesOrchestrator orchestrator, IHostingEnvironment hostingEnvironment)
        {
            _orchestrator = orchestrator;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet("edit-dates", Name = RouteNames.VacancyEditDates_Get)]
        public async Task<IActionResult> EditVacancyDates(VacancyRouteModel vrm)
        {
            var proposedClosingDate = Request.Cookies[string.Format(CookieNames.VacancyProposedClosingDate, vrm.VacancyId)]?.Trim();
            var proposedStartDate = Request.Cookies[string.Format(CookieNames.VacancyProposedStartDate, vrm.VacancyId)]?.Trim();
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

            Response.Cookies.Append(string.Format(CookieNames.VacancyProposedClosingDate, m.VacancyId), DateTime.Parse(m.ClosingDate).ToShortDateString(), EsfaCookieOptions.GetSessionLifetimeHttpCookieOption(_hostingEnvironment));
            Response.Cookies.Append(string.Format(CookieNames.VacancyProposedStartDate, m.VacancyId), DateTime.Parse(m.StartDate).ToShortDateString(), EsfaCookieOptions.GetSessionLifetimeHttpCookieOption(_hostingEnvironment));

            return RedirectToRoute(RouteNames.VacancyEdit_Get);
        }

        [HttpGet("cancel-edit-dates", Name = RouteNames.VacancyEditDatesCancel_Get)]
        public IActionResult CancelChanges(VacancyRouteModel vrm)
        {
            ClearEditDatesCookies(vrm.VacancyId);

            return RedirectToRoute(RouteNames.VacancyEdit_Get);
        }

        private void ClearEditDatesCookies(Guid vacancyId)
        {
            Response.Cookies.Delete(string.Format(CookieNames.VacancyProposedClosingDate, vacancyId), EsfaCookieOptions.GetSessionLifetimeHttpCookieOption(_hostingEnvironment));
            Response.Cookies.Delete(string.Format(CookieNames.VacancyProposedStartDate, vacancyId), EsfaCookieOptions.GetSessionLifetimeHttpCookieOption(_hostingEnvironment));
        }

        private IActionResult HandleRedirectOfDraftVacancy(Vacancy vacancy)
        {
            if (Utility.VacancyHasCompletedPartOne(vacancy))
            {
                if (Utility.VacancyHasStartedPartTwo(vacancy) == false)
                    return RedirectToRoute(RouteNames.SearchResultPreview_Get);

                return RedirectToRoute(RouteNames.Vacancy_Preview_Get);
            }

            var resumeRouteName = Utility.GetValidRoutesForVacancy(vacancy).Last();

            return RedirectToRoute(resumeRouteName);
        }
    }
}