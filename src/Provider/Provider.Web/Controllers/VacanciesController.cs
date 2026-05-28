#nullable enable
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

namespace Esfa.Recruit.Provider.Web.Controllers;

[Route(RoutePaths.VacanciesRoutePath)]
public class VacanciesController(VacanciesOrchestrator orchestrator, IWebHostEnvironment hostingEnvironment) : Controller
{
    private const int PageSize = 25;
    private const int MinPage = 1;
    private const int MaxPage = 9999;
    private static int ClampPage(int page) => Math.Clamp(page, MinPage, MaxPage);
    private const ColumnSortOrder DefaultSortOrder = ColumnSortOrder.Desc;

    [HttpGet("{filter?}", Name = RouteNames.VacanciesGetAll)]
    public async Task<IActionResult> ListVacancies(
        [FromRoute] int ukprn,
        SortParams<VacancySortColumn> sortParams,
        [FromRoute] FilteringOptions filter = FilteringOptions.All,
        [FromQuery] int? page = 1,
        [FromQuery] string? searchTerm = null)
    {
        var vm = await GetVacanciesViewModel(filter,
            User.ToVacancyUser(),
            searchTerm,
            page,
            sortParams.SortColumn,
            sortParams.SortOrder);
        
        return View("ListVacancies", vm);
    }

    private async Task<ListVacanciesViewModel> GetVacanciesViewModel(
        FilteringOptions filteringOption,
        VacancyUser user,
        string? searchTerm,
        int? page,
        VacancySortColumn sortColumn,
        ColumnSortOrder? sortOrder)
    {
        var vm = await orchestrator.ListVacanciesAsync(
            filteringOption,
            (int)user.Ukprn!.Value,
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
}