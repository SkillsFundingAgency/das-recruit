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
using Esfa.Recruit.Employer.Web.QueryStringModels;
using Esfa.Recruit.Employer.Web.ViewModels;

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

        [HttpGet("manage", Name = RouteNames.VacancyManage_Get)]
        public async Task<IActionResult> ManageVacancy(VacancyRouteModel vrm, [FromQuery]string proposedClosingDate, [FromQuery]string proposedStartDate)
        {
            var vacancy = await _orchestrator.GetVacancy(vrm);

            if (vacancy.CanEdit)
            {
                return HandleRedirectOfDraftVacancy(vacancy);
            }

            if (vacancy.Status != VacancyStatus.Live)
            {
                return RedirectToRoute(RouteNames.DisplayVacancy_Get);
            }

            DateTime parsedClosingDate = DateTime.MinValue;
            if (proposedClosingDate?.Length > 0)
            {
                if (DateTime.TryParseExact(proposedClosingDate, QueryString.Formats.DateFormat, null, System.Globalization.DateTimeStyles.None, out parsedClosingDate) == false)
                {
                    return RedirectToRoute(RouteNames.VacancyEditDates_Get, new EditDatesQueryStringModel { ProposedClosingDate = proposedClosingDate, ProposedStartDate = proposedStartDate });
                }
            }

            DateTime parsedStartDate = DateTime.MinValue;
            if (proposedStartDate?.Length > 0)
            {
                if (DateTime.TryParseExact(proposedStartDate, QueryString.Formats.DateFormat, null, System.Globalization.DateTimeStyles.None, out parsedStartDate) == false)
                {
                    return RedirectToRoute(RouteNames.VacancyEditDates_Get, new EditDatesQueryStringModel { ProposedClosingDate = proposedClosingDate, ProposedStartDate = proposedStartDate });
                }
            }

            var viewModel = await _orchestrator.GetViewModelForManageVacancy(vrm, parsedClosingDate, parsedStartDate);

            return View(viewModel);
        }

        [HttpPost("submit-vacancy-changes", Name = RouteNames.SubmitVacancyChanges_Post)]
        public async Task<IActionResult> UpdatePublishedVacancy(ProposedChangesEditModel m)
        {
            var response = await _orchestrator.UpdatePublishedVacancyAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                return RedirectToRoute(RouteNames.VacancyEditDates_Get, new EditDatesQueryStringModel { ProposedClosingDate = m.ProposedClosingDate, ProposedStartDate = m.ProposedStartDate });
            }

            var vacancy = await _orchestrator.GetVacancy(m);
            TempData.Add(TempDataKeys.DashboardInfoMessage, string.Format(InfoMessages.VacancyUpdated, vacancy.Title));
            return RedirectToRoute(RouteNames.Dashboard_Index_Get);
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