using System;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace Esfa.Recruit.Employer.Web.Controllers;

[Route(RoutePaths.VacanciesRoutePath)]
public class VacanciesController(VacanciesOrchestrator orchestrator, IWebHostEnvironment hostingEnvironment) : Controller
{
    private const int PageSize = 25;
    
    [HttpGet("all", Name = RouteNames.VacanciesGetAll)]
    public async Task<IActionResult> ListAllVacancies(
        [FromRoute] string employerAccountId,
        SortParams<VacancySortColumn> sortParams,
        [FromQuery] int? page = 1,
        [FromQuery] string searchTerm = null)
    {
        var user = User.ToVacancyUser();
        var vm = await orchestrator.ListAllVacanciesAsync(
            employerAccountId,
            user.Ukprn,
            user.UserId,
            page,
            PageSize,
            searchTerm,
            sortParams.SortColumn,
            sortParams.SortOrder);
    
        if (TempData.TryGetValue(TempDataKeys.DashboardErrorMessage, out var warningMessage))
        {
            vm.WarningMessage = warningMessage!.ToString();
        }
    
        if (TempData.TryGetValue(TempDataKeys.DashboardInfoMessage, out var infoMessage))
        {
            vm.InfoMessage = infoMessage!.ToString();
        }
    
        vm.ShowReferredFromMaBackLink = ShowReferredFromMaBackLink();
        return View(vm);
    }
    
    [HttpGet("", Name = RouteNames.Vacancies_Get)]
    public async Task<IActionResult> Vacancies([FromRoute] string employerAccountId, [FromQuery] string filter, [FromQuery] int page = 1, [FromQuery] string searchTerm = "")
    {
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