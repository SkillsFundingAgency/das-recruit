#nullable enable
using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.Vacancies;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers;

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
        [FromRoute] string employerAccountId,
        SortParams<VacancySortColumn> sortParams,
        [FromRoute] FilteringOptions filter = FilteringOptions.All,
        [FromQuery] int? page = 1,
        [FromQuery] string? searchTerm = null)
    {
        var vm = await GetVacanciesViewModel(filter,
            User.ToVacancyUser(),
            employerAccountId,
            searchTerm,
            page,
            sortParams.SortColumn,
            sortParams.SortOrder);

        return View("ListVacancies", vm);
    }
    
    private async Task<ListVacanciesViewModel> GetVacanciesViewModel(
        FilteringOptions filteringOption,
        VacancyUser user,
        string hashedEmployerAccountId,
        string? searchTerm,
        int? page,
        VacancySortColumn sortColumn,
        ColumnSortOrder? sortOrder)
    {
        var vm = await orchestrator.ListVacanciesAsync(
            filteringOption,
            hashedEmployerAccountId,
            (int?)user.Ukprn,
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
}