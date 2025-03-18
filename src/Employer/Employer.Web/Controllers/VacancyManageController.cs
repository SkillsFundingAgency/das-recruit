﻿using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyManage;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Middleware;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using InfoMsg = Esfa.Recruit.Shared.Web.ViewModels.InfoMessages;

namespace Esfa.Recruit.Employer.Web.Controllers;

[Route(RoutePaths.AccountVacancyRoutePath)]
[Authorize(Policy = nameof(PolicyNames.HasEmployerOwnerOrTransactorAccount))]
public class VacancyManageController(
    VacancyManageOrchestrator orchestrator,
    IWebHostEnvironment hostingEnvironment,
    IUtility utility) : Controller
{
    private const int PageSize = 1;
    
    [HttpGet("manage", Name = RouteNames.VacancyManage_Get)]
    public async Task<IActionResult> ManageVacancy(
        ManageVacancyRouteModel vrm,
        [FromQuery] string sortColumn,
        [FromQuery] string sortOrder,
        [FromQuery] bool vacancySharedByProvider,
        [FromQuery] string page)
    {
        EnsureProposedChangesCookiesAreCleared(vrm.VacancyId);
        var vacancy = await orchestrator.GetVacancy(vrm, vacancySharedByProvider);
        if (vacancy.CanEmployerEdit)
        {
            return HandleRedirectOfEditableVacancy(vacancy);
        }
        
        var pagingParams = GetPagingParams(sortColumn, sortOrder, page);
        var viewModel = await orchestrator.GetManageVacancyViewModel(vacancy, pagingParams, vacancySharedByProvider);

        if (TempData.TryGetValue(TempDataKeys.VacancyClosedMessage, out object closedMessage))
        {
            viewModel.VacancyClosedInfoMessage = closedMessage.ToString();
        }

        if (TempData.TryGetValue(TempDataKeys.ApplicationReviewStatusInfoMessage, out object applicationReviewStatusInfoMessage))
        {
            viewModel.EmployerReviewedApplicationHeaderMessage = applicationReviewStatusInfoMessage.ToString();
        }
            
        if (TempData.TryGetValue(TempDataKeys.ApplicationReviewedInfoMessage, out object applicationReviewedInfoMessage))
        {
            viewModel.EmployerReviewedApplicationBodyMessage = applicationReviewedInfoMessage.ToString();
        }

        if (TempData.TryGetValue(TempDataKeys.ApplicationReviewStatusChangeInfoMessage, out object applicationReviewStatusChangeInfoMessage))
        {
            viewModel.ApplicationStatusChangeHeaderMessage = applicationReviewStatusChangeInfoMessage.ToString();
        }

        if (TempData.TryGetValue(TempDataKeys.ApplicationReviewsUnsuccessfulInfoMessage, out object applicationReviewsUnsuccessfulInfoMessage))
        {
            viewModel.ApplicationReviewsUnsuccessfulBannerHeader = applicationReviewsUnsuccessfulInfoMessage.ToString();
        }

        return View(viewModel);
    }
    
    private static VacancyApplicationsPagingParams GetPagingParams(string sortColumn, string sortOrder, string page)
    {
        Enum.TryParse<SortOrder>(sortOrder, out var outputSortOrder);
        Enum.TryParse<SortColumn>(sortColumn, out var outputSortColumn);
        if (!int.TryParse(page, out int pageNumber))
        {
            pageNumber = 1;
        }
        
        return new VacancyApplicationsPagingParams(outputSortColumn, outputSortOrder, pageNumber, PageSize);
    }

    [HttpGet("edit", Name = RouteNames.VacancyEdit_Get)]
    public async Task<IActionResult> EditVacancy(VacancyRouteModel vrm)
    {
        var vacancy = await orchestrator.GetVacancy(vrm);

        if (vacancy.CanEmployerEdit)
        {
            return HandleRedirectOfEditableVacancy(vacancy);
        }

        if (vacancy.Status != VacancyStatus.Live)
        {
            return RedirectToRoute(RouteNames.DisplayVacancy_Get, new {vrm.VacancyId, vrm.EmployerAccountId});
        }

        var parsedClosingDate = Request.Cookies.GetProposedClosingDate(vacancy.Id);
        var parsedStartDate = Request.Cookies.GetProposedStartDate(vacancy.Id);

        var viewModel = await orchestrator.GetEditVacancyViewModel(vrm, parsedClosingDate, parsedStartDate);

        return View(viewModel);
    }

    [HttpPost("submit-vacancy-changes", Name = RouteNames.SubmitVacancyChanges_Post)]
    public async Task<IActionResult> UpdatePublishedVacancy(ProposedChangesEditModel m)
    {
        var response = await orchestrator.UpdatePublishedVacancyAsync(m, User.ToVacancyUser());

        if (!response.Success)
        {
            return RedirectToRoute(RouteNames.VacancyEditDates_Get, new {m.VacancyId, m.EmployerAccountId});
        }

        var vacancy = await orchestrator.GetVacancy(m);
        TempData.Add(TempDataKeys.DashboardInfoMessage, string.Format(InfoMsg.VacancyUpdated, vacancy.Title));

        EnsureProposedChangesCookiesAreCleared(m.VacancyId);

        return RedirectToRoute(RouteNames.Vacancies_Get, new {m.VacancyId, m.EmployerAccountId});
    }

    [HttpGet("cancel-vacancy-changes", Name = RouteNames.CancelVacancyChanges_Get)]
    public IActionResult CancelChanges(VacancyRouteModel vrm)
    {
        EnsureProposedChangesCookiesAreCleared(vrm.VacancyId);
            
        return RedirectToRoute(RouteNames.Vacancies_Get, new {vrm.VacancyId, vrm.EmployerAccountId});
    }

    private void EnsureProposedChangesCookiesAreCleared(Guid vacancyId)
    {
        Response.Cookies.ClearProposedClosingDate(hostingEnvironment, vacancyId);
        Response.Cookies.ClearProposedStartDate(hostingEnvironment, vacancyId);
    }

    private IActionResult HandleRedirectOfEditableVacancy(Vacancy vacancy)
    {
        if (utility.IsTaskListCompleted(vacancy))
        {
            return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new {VacancyId = vacancy.Id, vacancy.EmployerAccountId});
        }
        return RedirectToRoute(RouteNames.EmployerTaskListGet, new {VacancyId = vacancy.Id, vacancy.EmployerAccountId});
    }
}