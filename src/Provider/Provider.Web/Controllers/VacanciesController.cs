using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Esfa.Recruit.Provider.Web.Controllers;

[Route(RoutePaths.VacanciesRoutePath)]
public class VacanciesController(VacanciesOrchestrator orchestrator, IWebHostEnvironment hostingEnvironment) : Controller
{
    [HttpGet("all", Name = RouteNames.Vacancies_GetAll)]
    public async Task<IActionResult> ListAllVacancies(
        [FromRoute] int ukprn,
        SortParams<VacancySortColumn> sortParams,
        [FromQuery] int? page = 1,
        [FromQuery] string searchTerm = null)
    {
        const int pageSize = 10;
        var vm = await orchestrator.ListAllVacanciesAsync(
            ukprn,
            User.ToVacancyUser().UserId,
            page,
            pageSize,
            searchTerm,
            sortParams.SortColumn,
            sortParams.SortOrder);

        if (TempData.TryGetValue(TempDataKeys.VacanciesErrorMessage, out var warningMessage))
        {
            vm.WarningMessage = warningMessage!.ToString();
        }

        if (TempData.TryGetValue(TempDataKeys.VacanciesInfoMessage, out var infoMessage))
        {
            vm.InfoMessage = infoMessage!.ToString();
        }

        return View(vm);
    }

    [HttpGet("", Name = RouteNames.Vacancies_Get)]
    public async Task<IActionResult> Vacancies([FromRoute] int ukprn, [FromQuery] string filter, [FromQuery] int page = 1, [FromQuery] string searchTerm = "")
    {
        if (filter.Equals("all", StringComparison.CurrentCultureIgnoreCase))
        {
            return RedirectToRoute(RouteNames.Vacancies_GetAll, new { ukprn });
        }
        
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