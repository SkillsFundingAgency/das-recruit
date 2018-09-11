using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyManage;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.ViewModels;
using Microsoft.AspNetCore.Hosting;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class VacancyManageController : Controller
    {
        private readonly VacancyManageOrchestrator _orchestrator;
        private readonly IHostingEnvironment _hostingEnvironment;

        public VacancyManageController(VacancyManageOrchestrator orchestrator, IHostingEnvironment hostingEnvironment)
        {
            _orchestrator = orchestrator;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet("manage", Name = RouteNames.VacancyManage_Get)]
        public async Task<IActionResult> ManageVacancy(VacancyRouteModel vrm)
        {
            var proposedClosingDate = Request.Cookies[string.Format(CookieNames.VacancyProposedClosingDate, vrm.VacancyId)]?.Trim();
            var proposedStartDate = Request.Cookies[string.Format(CookieNames.VacancyProposedStartDate, vrm.VacancyId)]?.Trim();
            var vacancy = await _orchestrator.GetVacancy(vrm);

            if (vacancy.CanEdit)
            {
                return HandleRedirectOfEditableVacancy(vacancy);
            }

            if (vacancy.Status != VacancyStatus.Live)
            {
                return RedirectToRoute(RouteNames.DisplayVacancy_Get);
            }

            DateTime parsedClosingDate = DateTime.MinValue;
            if (proposedClosingDate?.Length > 0 && DateTime.TryParse(proposedClosingDate, out parsedClosingDate) == false)
                return RedirectToRoute(RouteNames.VacancyEditDates_Get);

            DateTime parsedStartDate = DateTime.MinValue;
            if (proposedStartDate?.Length > 0 && DateTime.TryParse(proposedStartDate, out parsedStartDate) == false)
                return RedirectToRoute(RouteNames.VacancyEditDates_Get);

            var viewModel = await _orchestrator.GetViewModelForManageVacancy(vrm, parsedClosingDate, parsedStartDate);

            return View(viewModel);
        }

        [HttpPost("submit-vacancy-changes", Name = RouteNames.SubmitVacancyChanges_Post)]
        public async Task<IActionResult> UpdatePublishedVacancy(ProposedChangesEditModel m)
        {
            var response = await _orchestrator.UpdatePublishedVacancyAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                return RedirectToRoute(RouteNames.VacancyEditDates_Get);
            }

            var vacancy = await _orchestrator.GetVacancy(m);
            TempData.Add(TempDataKeys.DashboardInfoMessage, string.Format(InfoMessages.VacancyUpdated, vacancy.Title));
            ClearEditDatesCookies(m.VacancyId);

            return RedirectToRoute(RouteNames.Dashboard_Index_Get);
        }

        [HttpGet("cancel-vacancy-changes", Name = RouteNames.CancelVacancyChanges_Get)]
        public IActionResult CancelChanges(VacancyRouteModel vrm)
        {
            ClearEditDatesCookies(vrm.VacancyId);

            return RedirectToRoute(RouteNames.Dashboard_Index_Get);
        }

        private void ClearEditDatesCookies(Guid vacancyId)
        {
            Response.Cookies.Delete(string.Format(CookieNames.VacancyProposedClosingDate, vacancyId), EsfaCookieOptions.GetSessionLifetimeHttpCookieOption(_hostingEnvironment));
            Response.Cookies.Delete(string.Format(CookieNames.VacancyProposedStartDate, vacancyId), EsfaCookieOptions.GetSessionLifetimeHttpCookieOption(_hostingEnvironment));
        }

        private IActionResult HandleRedirectOfEditableVacancy(Vacancy vacancy)
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