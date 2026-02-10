using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.ViewModels.Vacancies;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Esfa.Recruit.Provider.Web.Controllers;

[Route(RoutePaths.VacanciesRoutePath)]
public class VacanciesController(VacanciesOrchestrator orchestrator, IWebHostEnvironment hostingEnvironment) : Controller
{
    private const int PageSize = 25;
    private const int MinPage = 1;
    private const int MaxPage = 9999;
    private static int ClampPage(int page) => Math.Clamp(page, MinPage, MaxPage);
    private const ColumnSortOrder DefaultSortOrder = ColumnSortOrder.Desc;
    
    [HttpGet("all", Name = RouteNames.VacanciesGetAll)]
    public async Task<IActionResult> ListAllVacancies(
        [FromRoute] int ukprn,
        SortParams<VacancySortColumn> sortParams,
        [FromQuery] int? page = 1,
        [FromQuery] string searchTerm = null)
    {
        var user = User.ToVacancyUser();
        if ((int)user.Ukprn!.Value != ukprn)
        {
            throw new Exception($"User does not have access to list 'all' vacancies for provider {ukprn}");
        }
        
        var vm = await GetVacanciesViewModel(FilteringOptions.All, user, searchTerm, page, sortParams.SortColumn, sortParams.SortOrder);
        return View("ListVacancies", vm);
    }
    
    [HttpGet("draft", Name = RouteNames.VacanciesListDraft)]
    public async Task<IActionResult> ListDraftVacancies(
        [FromRoute] int ukprn,
        SortParams<VacancySortColumn> sortParams,
        [FromQuery] int? page = 1,
        [FromQuery] string searchTerm = null)
    {
        var user = User.ToVacancyUser();
        if ((int)user.Ukprn!.Value != ukprn)
        {
            throw new Exception($"User does not have access to list 'draft' vacancies for provider {ukprn}");
        }
        
        var vm = await GetVacanciesViewModel(FilteringOptions.Draft, user, searchTerm, page, sortParams.SortColumn, sortParams.SortOrder);
        return View("ListVacancies", vm);
    }

    private async Task<ListVacanciesViewModel> GetVacanciesViewModel(
        FilteringOptions filteringOption,
        VacancyUser user,
        string searchTerm,
        int? page,
        VacancySortColumn sortColumn,
        ColumnSortOrder? sortOrder)
    {
        var vm = await orchestrator.ListVacanciesAsync(
            filteringOption,
            (int)user.Ukprn!.Value,
            user.UserId,
            searchTerm?.Trim(),
            ClampPage(page ?? MinPage),
            PageSize,
            sortColumn,
            sortOrder ?? DefaultSortOrder);

        if (TempData.TryGetValue(TempDataKeys.VacanciesErrorMessage, out var warningMessage))
        {
            vm.WarningMessage = warningMessage!.ToString();
        }

        if (TempData.TryGetValue(TempDataKeys.VacanciesInfoMessage, out var infoMessage))
        {
            vm.InfoMessage = infoMessage!.ToString();
        }
        
        return vm;
    }

    [HttpGet("", Name = RouteNames.Vacancies_Get)]
    public async Task<IActionResult> Vacancies([FromRoute] int ukprn, [FromQuery] string filter, [FromQuery] int page = 1, [FromQuery] string searchTerm = "")
    {
        // TODO: Comment back in for late stage testing - leave in so testers can compare data on the new to old page
        // if (filter.Equals("all", StringComparison.CurrentCultureIgnoreCase))
        // {
        //     return RedirectToRoute(RouteNames.VacanciesGetAll, new { ukprn });
        // }
        
        if (string.IsNullOrWhiteSpace(filter) && string.IsNullOrWhiteSpace(searchTerm))
            TryGetFiltersFromCookie(out filter, out searchTerm);
            
        if(string.IsNullOrWhiteSpace(filter) == false || string.IsNullOrWhiteSpace(searchTerm) == false)
            SaveFiltersInCookie(filter, searchTerm);

        var vm = await orchestrator.GetVacanciesViewModelAsync(User.ToVacancyUser(), filter, page, searchTerm);
        if (TempData.ContainsKey(TempDataKeys.VacanciesErrorMessage))
            vm.WarningMessage = TempData[TempDataKeys.VacanciesErrorMessage].ToString();

        if (TempData.ContainsKey(TempDataKeys.VacanciesInfoMessage))
            vm.InfoMessage = TempData[TempDataKeys.VacanciesInfoMessage].ToString();

        return View(vm);
    }
        
    private void SaveFiltersInCookie(string filter, string search)
    {
        var value = JsonConvert.SerializeObject(new FilterCookie(filter, search));
        Response.Cookies.SetSessionCookie(hostingEnvironment, CookieNames.VacanciesFilter, value);
    }

    private void TryGetFiltersFromCookie(out string filter, out string search)
    {
        filter = string.Empty;
        search = string.Empty;
        var cookieValue = Request.Cookies.GetCookie(CookieNames.VacanciesFilter);
        if (string.IsNullOrWhiteSpace(cookieValue)) return;
        var values = JsonConvert.DeserializeObject<FilterCookie>(cookieValue);
        filter = values.Filter;
        search = values.SearchTerm;
    }

    private class FilterCookie
    {
        public string Filter { get; set; }
        public string SearchTerm { get; set; }
        public FilterCookie() { }
        public FilterCookie(string filter, string searchTerm)
        {
            Filter = filter;
            SearchTerm = searchTerm;
        }
    }
}