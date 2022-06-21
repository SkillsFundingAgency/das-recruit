﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyManage;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using InfoMsg = Esfa.Recruit.Shared.Web.ViewModels.InfoMessages;

namespace Esfa.Recruit.Provider.Web.Controllers
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
            EnsureProposedChangesCookiesAreCleared(vrm.VacancyId.GetValueOrDefault());

            var vacancy = await _orchestrator.GetVacancy(vrm);

            if (vacancy.CanEdit)
            {
                return HandleRedirectOfEditableVacancy(vacancy);
            }

            var viewModel = await _orchestrator.GetManageVacancyViewModel(vacancy, vrm);

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

            if (vacancy.CanEdit)
            {
                return HandleRedirectOfEditableVacancy(vacancy);
            }

            if (vacancy.Status != VacancyStatus.Live)
            {
                return RedirectToRoute(RouteNames.DisplayVacancy_Get, new {vrm.VacancyId, vrm.Ukprn});
            }

            var parsedClosingDate = Request.Cookies.GetProposedClosingDate(vacancy.Id);
            var parsedStartDate = Request.Cookies.GetProposedStartDate(vacancy.Id);

            var viewModel = await _orchestrator.GetEditVacancyViewModel(vrm, parsedClosingDate, parsedStartDate);

            return View(viewModel);
        }

        [HttpPost("submit-vacancy-changes", Name = RouteNames.SubmitVacancyChanges_Post)]
        [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
        public async Task<IActionResult> UpdatePublishedVacancy(ProposedChangesEditModel m)
        {
            var response = await _orchestrator.UpdatePublishedVacancyAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                return RedirectToRoute(RouteNames.VacancyEditDates_Get, new {m.VacancyId, m.Ukprn});
            }

            var vacancy = await _orchestrator.GetVacancy(m);
            TempData.Add(TempDataKeys.VacanciesInfoMessage, string.Format(InfoMsg.VacancyUpdated, vacancy.Title));

            EnsureProposedChangesCookiesAreCleared(m.VacancyId.GetValueOrDefault());

            return RedirectToRoute(RouteNames.Vacancies_Get, new {m.Ukprn});
        }

        [HttpGet("cancel-vacancy-changes", Name = RouteNames.CancelVacancyChanges_Get)]
        public IActionResult CancelChanges(VacancyRouteModel vrm)
        {
            EnsureProposedChangesCookiesAreCleared(vrm.VacancyId.GetValueOrDefault());
            
            return RedirectToRoute(RouteNames.Vacancies_Get, new {vrm.Ukprn});
        }

        private void EnsureProposedChangesCookiesAreCleared(Guid vacancyId)
        {
            Response.Cookies.ClearProposedClosingDate(_hostingEnvironment, vacancyId);
            Response.Cookies.ClearProposedStartDate(_hostingEnvironment, vacancyId);
        }

        private IActionResult HandleRedirectOfEditableVacancy(Vacancy vacancy)
        {
            if (_utility.TaskListCompleted(vacancy))
            {
                return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new {vacancy.TrainingProvider.Ukprn, vacancyId = vacancy.Id});
            }
            return RedirectToRoute(RouteNames.ProviderTaskListGet, new {vacancy.TrainingProvider.Ukprn, vacancyId = vacancy.Id});
        }
    }
}