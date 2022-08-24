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
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Microsoft.AspNetCore.Hosting;
using InfoMsg = Esfa.Recruit.Shared.Web.ViewModels.InfoMessages;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class VacancyManageController : Controller
    {
        private readonly VacancyManageOrchestrator _orchestrator;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUtility _utility;
        private readonly IFeature _feature;

        public VacancyManageController(VacancyManageOrchestrator orchestrator, IHostingEnvironment hostingEnvironment, IUtility utility, IFeature feature)
        {
            _orchestrator = orchestrator;
            _hostingEnvironment = hostingEnvironment;
            _utility = utility;
            _feature = feature;
        }

        [HttpGet("manage", Name = RouteNames.VacancyManage_Get)]
        public async Task<IActionResult> ManageVacancy(VacancyRouteModel vrm)
        {
            EnsureProposedChangesCookiesAreCleared(vrm.VacancyId);

            var vacancy = await _orchestrator.GetVacancy(vrm);

            if (vacancy.CanEmployerEdit)
            {
                return HandleRedirectOfEditableVacancy(vacancy);
            }

            var viewModel = await _orchestrator.GetManageVacancyViewModel(vacancy);

            if (TempData.ContainsKey(TempDataKeys.VacancyClosedMessage))
                viewModel.VacancyClosedInfoMessage = TempData[TempDataKeys.VacancyClosedMessage].ToString();

            if (TempData.ContainsKey(TempDataKeys.ApplicationReviewStatusInfoMessage))
                viewModel.ApplicationReviewStatusHeaderInfoMessage = TempData[TempDataKeys.ApplicationReviewStatusInfoMessage].ToString();

            return View(viewModel);
        }

        [HttpGet("edit", Name = RouteNames.VacancyEdit_Get)]
        public async Task<IActionResult> EditVacancy(VacancyRouteModel vrm)
        {
            var vacancy = await _orchestrator.GetVacancy(vrm);

            if (vacancy.CanEmployerEdit)
            {
                return HandleRedirectOfEditableVacancy(vacancy);
            }

            if (vacancy.Status != VacancyStatus.Live)
            {
                return RedirectToRoute(RouteNames.DisplayVacancy_Get);
            }

            var parsedClosingDate = Request.Cookies.GetProposedClosingDate(vacancy.Id);
            var parsedStartDate = Request.Cookies.GetProposedStartDate(vacancy.Id);

            var viewModel = await _orchestrator.GetEditVacancyViewModel(vrm, parsedClosingDate, parsedStartDate);

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
            TempData.Add(TempDataKeys.DashboardInfoMessage, string.Format(InfoMsg.VacancyUpdated, vacancy.Title));

            EnsureProposedChangesCookiesAreCleared(m.VacancyId);

            return RedirectToRoute(RouteNames.Vacancies_Get);
        }

        [HttpGet("cancel-vacancy-changes", Name = RouteNames.CancelVacancyChanges_Get)]
        public IActionResult CancelChanges(VacancyRouteModel vrm)
        {
            EnsureProposedChangesCookiesAreCleared(vrm.VacancyId);
            
            return RedirectToRoute(RouteNames.Vacancies_Get);
        }

        private void EnsureProposedChangesCookiesAreCleared(Guid vacancyId)
        {
            Response.Cookies.ClearProposedClosingDate(_hostingEnvironment, vacancyId);
            Response.Cookies.ClearProposedStartDate(_hostingEnvironment, vacancyId);
        }

        private IActionResult HandleRedirectOfEditableVacancy(Vacancy vacancy)
        {
            if (_utility.IsTaskListCompleted(vacancy))
            {
                return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet);
            }
            return RedirectToRoute(RouteNames.EmployerTaskListGet);
       
        }
    }
}