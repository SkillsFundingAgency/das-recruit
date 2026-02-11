using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.ViewModels.Vacancies;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace Esfa.Recruit.Employer.Web.Controllers;

[Route(RoutePaths.VacanciesRoutePath)]
public class VacanciesController(VacanciesOrchestrator orchestrator, IWebHostEnvironment hostingEnvironment) : Controller
{
    private const ColumnSortOrder DefaultSortOrder = ColumnSortOrder.Desc;
    private const int PageSize = 25;
    private const int MinPage = 1;
    private const int MaxPage = 9999;
    private static int ClampPage(int page) => Math.Clamp(page, MinPage, MaxPage);
    
    [HttpGet("all", Name = RouteNames.VacanciesGetAll)]
    public async Task<IActionResult> ListAllVacancies(
        [FromRoute] string employerAccountId,
        SortParams<VacancySortColumn> sortParams,
        [FromQuery] int? page = 1,
        [FromQuery] string searchTerm = null)
    {
        var vm = await GetVacanciesViewModel(FilteringOptions.All, User.ToVacancyUser(), employerAccountId, searchTerm, page, sortParams.SortColumn, sortParams.SortOrder);
        return View("ListVacancies", vm);
    }
    
    [HttpGet("draft", Name = RouteNames.VacanciesListDraft)]
    public async Task<IActionResult> ListDraftVacancies(
        [FromRoute] string employerAccountId,
        SortParams<VacancySortColumn> sortParams,
        [FromQuery] int? page = 1,
        [FromQuery] string searchTerm = null)
    {
        var vm = await GetVacanciesViewModel(FilteringOptions.Draft, User.ToVacancyUser(), employerAccountId, searchTerm, page, sortParams.SortColumn, sortParams.SortOrder);
        return View("ListVacancies", vm);
    }
    
    private async Task<ListVacanciesViewModel> GetVacanciesViewModel(
        FilteringOptions filteringOption,
        VacancyUser user,
        string hashedEmployerAccountId,
        string searchTerm,
        int? page,
        VacancySortColumn sortColumn,
        ColumnSortOrder? sortOrder)
    {
        var vm = await orchestrator.ListVacanciesAsync(
            filteringOption,
            hashedEmployerAccountId,
            (int?)user.Ukprn,
            user.UserId,
            searchTerm?.Trim(),
            ClampPage(page ?? MinPage),
            PageSize,
            sortColumn,
            sortOrder ?? DefaultSortOrder);

        if (TempData.TryGetValue(TempDataKeys.DashboardErrorMessage, out var warningMessage))
        {
            vm.WarningMessage = warningMessage!.ToString();
        }
    
        if (TempData.TryGetValue(TempDataKeys.DashboardInfoMessage, out var infoMessage))
        {
            vm.InfoMessage = infoMessage!.ToString();
        }
        
        return vm;
    }

    [HttpGet("", Name = RouteNames.Vacancies_Get)]
    public async Task<IActionResult> Vacancies([FromRoute] string employerAccountId, [FromQuery] string filter, [FromQuery] int page = 1, [FromQuery] string searchTerm = "")
    {
        // TODO: Comment back in for late stage testing - leave in so testers can compare data on the new to old page
        // var blocked = new List<string> {"all", "draft"};
        // if (string.IsNullOrWhiteSpace(filter) || blocked.Any(x => filter.Equals(x, StringComparison.CurrentCultureIgnoreCase)))
        // {
        //     return RedirectToRoute(RouteNames.VacanciesGetAll, new { employerAccountId });
        // }
        
        if (string.IsNullOrWhiteSpace(filter) && string.IsNullOrWhiteSpace(searchTerm))
            TryGetFiltersFromCookie(out filter, out searchTerm);

        if (string.IsNullOrWhiteSpace(filter) == false || string.IsNullOrWhiteSpace(searchTerm) == false)
            SaveFiltersInCookie(filter, searchTerm);

        var vm = await orchestrator.GetVacanciesViewModelAsync(employerAccountId, filter, page, User.ToVacancyUser(), searchTerm);

        if (TempData.ContainsKey(TempDataKeys.DashboardErrorMessage))
            vm.WarningMessage = TempData[TempDataKeys.DashboardErrorMessage].ToString();

        if (TempData.ContainsKey(TempDataKeys.DashboardInfoMessage))
            vm.InfoMessage = TempData[TempDataKeys.DashboardInfoMessage].ToString();

        vm.ShowReferredFromMaBackLink = ShowReferredFromMaBackLink();
        return View(vm);
    }

    private void SaveFiltersInCookie(string filter, string search)
    {
        var value = JsonConvert.SerializeObject(new FilterCookie(filter, search));
        Response.Cookies.SetSessionCookie(hostingEnvironment, CookieNames.DashboardFilter, value);
    }

    private void TryGetFiltersFromCookie(out string filter, out string search)
    {
        filter = string.Empty;
        search = string.Empty;
        var cookieValue = Request.Cookies.GetCookie(CookieNames.DashboardFilter);
        if (string.IsNullOrWhiteSpace(cookieValue)) return;
        try
        {
            var values = JsonConvert.DeserializeObject<FilterCookie>(cookieValue);
            filter = values.Filter;
            search = values.SearchTerm;
        }
        catch (JsonException)
        {
            //As the cookie value was initially set as string, we need to handle the deserialization in a try/catch block.
        }
    }

    private class FilterCookie
    {
        public string Filter { get; }
        public string SearchTerm { get; }
        public FilterCookie(string filter, string searchTerm)
        {
            Filter = filter;
            SearchTerm = searchTerm;
        }
    }

    private bool ShowReferredFromMaBackLink()
    {
        return Convert.ToBoolean(TempData.Peek(TempDataKeys.ReferredFromMa));
    }
}