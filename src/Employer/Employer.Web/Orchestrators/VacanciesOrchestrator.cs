using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.ViewModels.Alerts;
using Esfa.Recruit.Employer.Web.ViewModels.Vacancies;
using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Vacancy.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Vacancies;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Employer.Web.Orchestrators;

public class VacanciesOrchestrator(IEmployerVacancyClient vacancyClient,
    IEmployerAccountProvider employerAccountProvider,
    IOuterApiClient outerApiClient,
    IEncodingService encodingService)
{
    private const int VacanciesPerPage = 25;

    public async Task<VacanciesViewModel> GetVacanciesViewModelAsync(string employerAccountId, string filter, int page, VacancyUser user, string searchTerm)
    {
        var filteringOption = SanitizeFilter(filter);
        var employerDashboard = await vacancyClient.GetDashboardAsync(employerAccountId, user.UserId, page, VacanciesPerPage, "CreatedDate", "Desc", filteringOption, searchTerm);

        var totalItems = Convert.ToInt32(employerDashboard.TotalVacancies);

        var vacancies = new List<VacancySummary>(employerDashboard.Vacancies ?? []);
        page = SanitizePage(page, totalItems);

        var vacanciesVm = vacancies
            .Select(VacancySummaryMapper.ConvertToVacancySummaryViewModel)
            .ToList();

        var alerts = new AlertsViewModel(
            new EmployerTransferredVacanciesAlertViewModel
            {
                TransferredVacanciesCount = employerDashboard.EmployerRevokedTransferredVacanciesAlert
                    .TransferredVacanciesCount,
                EmployerAccountId = employerAccountId,
                TransferredVacanciesProviderNames = employerDashboard.EmployerRevokedTransferredVacanciesAlert
                    .TransferredVacanciesProviderNames,

            },
            new EmployerTransferredVacanciesAlertViewModel
            {
                EmployerAccountId = employerAccountId,
                TransferredVacanciesCount = employerDashboard.BlockedProviderTransferredVacanciesAlert
                    .TransferredVacanciesCount,
                TransferredVacanciesProviderNames = employerDashboard.BlockedProviderTransferredVacanciesAlert
                    .TransferredVacanciesProviderNames
            },
            new BlockedProviderAlertViewModel
            {
                EmployerAccountId = employerAccountId,
                BlockedProviderNames = employerDashboard.BlockedProviderAlert.BlockedProviderNames,
                ClosedVacancies = employerDashboard.BlockedProviderAlert.ClosedVacancies,
            },
            new WithdrawnVacanciesAlertViewModel
            {
                EmployerAccountId = employerAccountId,
                ClosedVacancies = employerDashboard.WithDrawnByQaVacanciesAlert.ClosedVacancies,
                Ukprn = user.Ukprn.GetValueOrDefault()
            }
        );

        var pager = new PagerViewModel(
            totalItems,
            VacanciesPerPage,
            page,
            "Showing {0} to {1} of {2} adverts",
            RouteNames.Vacancies_Get,
            new Dictionary<string, string>
            {
                {"filter", filteringOption.ToString()},
                {"searchTerm", searchTerm}
            });

        var vm = new VacanciesViewModel
        {
            EmployerAccountId = employerAccountId,
            Vacancies = vacanciesVm,
            Pager = pager,
            Filter = filteringOption,
            SearchTerm = searchTerm,
            ResultsHeading = VacancyFilterHeadingHelper.GetFilterHeading(Constants.VacancyTerm, totalItems, filteringOption, searchTerm, UserType.Employer),
            Alerts = alerts
        };

        return vm;
    }

    private static int SanitizePage(int page, int totalVacancies) => page < 0 || page > (int)Math.Ceiling((double)totalVacancies / VacanciesPerPage) ? 1 : page;

    private static FilteringOptions SanitizeFilter(string filter)
    {
        if (Enum.TryParse(typeof(FilteringOptions), filter, out var status))
            return (FilteringOptions)status;
        return FilteringOptions.All;
    }

    public async Task<ListVacanciesViewModel> ListVacanciesAsync(
        FilteringOptions filteringOption,
        string hashedEmployerAccountId,
        int? ukprn,
        string userId,
        string? searchTerm = null,
        int page = 1,
        int pageSize = VacanciesPerPage,
        VacancySortColumn sortColumn = VacancySortColumn.CreatedDate,
        ColumnSortOrder sortOrder = ColumnSortOrder.Desc)
    {
        var alertsTask = employerAccountProvider.GetEmployerAlerts(hashedEmployerAccountId, userId);
        var pageHeading = GetPageHeading(filteringOption);
        var result = await GetVacancies(filteringOption, hashedEmployerAccountId, searchTerm, page, pageSize, sortColumn, sortOrder);
        var totalItems = Convert.ToInt32(result.PageInfo.TotalCount);

        // this is our base route
        var baseRouteDictionary = new Dictionary<string, string> { ["employerAccountId"] = hashedEmployerAccountId };

        // create a separate dict with search params included
        var routeDictionary = new Dictionary<string, string>(baseRouteDictionary);
        if (sortColumn is not VacancySortColumn.CreatedDate) // ignore default
        {
            routeDictionary.Add("sortColumn", $"{sortColumn}");
            {
                // only order if the sort column is set
                routeDictionary.Add("sortOrder", $"{sortOrder}");
            }
        }
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            routeDictionary.Add("searchTerm", searchTerm);
        }

        var alerts = await alertsTask;
        return new ListVacanciesViewModel
        {
            Alerts = new AlertsViewModel(
                new EmployerTransferredVacanciesAlertViewModel
                {
                    EmployerAccountId = hashedEmployerAccountId,
                    TransferredVacanciesCount = alerts.EmployerRevokedTransferredVacanciesAlert.TransferredVacanciesCount,
                    TransferredVacanciesProviderNames = alerts.EmployerRevokedTransferredVacanciesAlert.TransferredVacanciesProviderNames,

                },
                new EmployerTransferredVacanciesAlertViewModel
                {
                    EmployerAccountId = hashedEmployerAccountId,
                    TransferredVacanciesCount = alerts.BlockedProviderTransferredVacanciesAlert.TransferredVacanciesCount,
                    TransferredVacanciesProviderNames = alerts.BlockedProviderTransferredVacanciesAlert.TransferredVacanciesProviderNames
                },
                new BlockedProviderAlertViewModel
                {
                    EmployerAccountId = hashedEmployerAccountId,
                    BlockedProviderNames = alerts.BlockedProviderAlert.BlockedProviderNames,
                    ClosedVacancies = alerts.BlockedProviderAlert.ClosedVacancies,
                },
                new WithdrawnVacanciesAlertViewModel
                {
                    EmployerAccountId = hashedEmployerAccountId,
                    ClosedVacancies = alerts.WithDrawnByQaVacanciesAlert.ClosedVacancies,
                    Ukprn = ukprn.GetValueOrDefault()
                }
            ),
            FilterViewModel = new VacanciesListSearchFilterViewModel
            {
                ResultsHeading = VacancyFilterHeadingHelper.GetFilterHeading(Constants.VacancyTerm, totalItems, filteringOption, searchTerm, UserType.Employer),
                SearchTerm = searchTerm,
                SuggestionsEnabled = false, // TODO: disable for the moment it doesn't take into account the vacancy status, so would suggest things not in the list
                SuggestionsRoute = RouteNames.VacanciesSearchSuggestions_Get,
                SuggestionsRouteDictionary = routeDictionary,
                RouteDictionary = baseRouteDictionary,
                UserType = UserType.Employer,
            },
            ListViewModel = new VacanciesListViewModel
            {
                EditVacancyRoute = RouteNames.EmployerTaskListGet,
                ManageVacancyRoute = RouteNames.VacancyManage_Get,
                Pagination = new PaginationViewModel(totalItems, pageSize, page, "Showing {0} to {1} of {2} adverts"),
                RouteDictionary = routeDictionary,
                ShowEmployerReviewedApplicationCounts = false,
                ShowSourceOrigin = true,
                SortColumn = sortColumn,
                SortOrder = sortOrder,
                SubmitVacancyRoute = RouteNames.EmployerCheckYourAnswersGet,
                Vacancies = result.Data.Select(x => VacancyListItemViewModel.From(x, hashedEmployerAccountId, filteringOption)).ToList(),
                UserType = UserType.Employer,
                Filter = filteringOption,
            },
            PageHeading = pageHeading,
            EmployerAccountId = hashedEmployerAccountId
        };
    }

    private static readonly HashSet<FilteringOptions> ApplicationListOptions =
    [
        FilteringOptions.NewApplications,
        FilteringOptions.AllApplications,
        FilteringOptions.ClosingSoon,
        FilteringOptions.ClosingSoonWithNoApplications,
        FilteringOptions.Transferred,
        FilteringOptions.EmployerReviewedApplications,
        FilteringOptions.NewSharedApplications,
        FilteringOptions.AllSharedApplications,
        FilteringOptions.Dashboard
    ];

    private async Task<PagedDataResponse<IEnumerable<VacancyListItem>>> GetVacancies(FilteringOptions options,
        string hashedEmployerAccountId,
        string searchTerm,
        int page,
        int pageSize,
        VacancySortColumn sortColumn,
        ColumnSortOrder sortOrder)
    {
        if (!Enum.IsDefined(typeof(FilteringOptions), options))
            throw new ArgumentOutOfRangeException(nameof(options), options, null);

        return ApplicationListOptions.Contains(options)
            ? await GetVacanciesListIncludingApplicationsAsync(options, hashedEmployerAccountId, searchTerm, page, pageSize, sortColumn, sortOrder)
            : await GetVacanciesList(options, hashedEmployerAccountId, searchTerm, page, pageSize, sortColumn, sortOrder);
    }

    private async Task<PagedDataResponse<IEnumerable<VacancyListItem>>> GetVacanciesList(FilteringOptions options,
        string hashedEmployerAccountId,
        string searchTerm,
        int page,
        int pageSize,
        VacancySortColumn sortColumn,
        ColumnSortOrder sortOrder)
    {
        var request = GetVacanciesListRequest(options, hashedEmployerAccountId, searchTerm, page, pageSize, sortColumn, sortOrder);
        return await outerApiClient.Get<PagedDataResponse<IEnumerable<VacancyListItem>>>(request);
    }

    private async Task<PagedDataResponse<IEnumerable<VacancyListItem>>> GetVacanciesListIncludingApplicationsAsync(FilteringOptions options,
        string hashedEmployerAccountId,
        string searchTerm,
        int page,
        int pageSize,
        VacancySortColumn sortColumn,
        ColumnSortOrder sortOrder)
    {
        var employerAccountId = encodingService.Decode(hashedEmployerAccountId, EncodingType.AccountId);
        var request = new GetVacanciesByAccountIdApiRequest(employerAccountId,
            page,
            pageSize,
            sortColumn.ToString(),
            sortOrder.ToString(),
            options,
            searchTerm);
        var response = await outerApiClient.Get<GetVacanciesByAccountIdApiResponse>(request);
        var items = response.VacancySummaries.Select(x => (VacancyListItem)x).ToList();
        return new PagedDataResponse<IEnumerable<VacancyListItem>>(items,
            new PageInfo(
                (ushort)page,
                (ushort)pageSize,
                (uint)response.PageInfo.TotalCount));
    }

    private GetVacanciesByEmployerAccountAndStatusApiRequest GetVacanciesListRequest(
        FilteringOptions options,
        string hashedEmployerAccountId,
        string searchTerm,
        int page,
        int pageSize,
        VacancySortColumn sortColumn,
        ColumnSortOrder sortOrder)
    {
        var employerAccountId = encodingService.Decode(hashedEmployerAccountId, EncodingType.AccountId);
        return !Enum.IsDefined(typeof(FilteringOptions), options) 
            ? throw new ArgumentOutOfRangeException(nameof(options), options, null) 
            : new GetVacanciesByEmployerAccountAndStatusApiRequest(employerAccountId, searchTerm, page, pageSize, options, sortColumn, sortOrder);
    }

    private static string GetPageHeading(FilteringOptions filteringOption) =>
        filteringOption switch
        {
            FilteringOptions.All => "All adverts",
            FilteringOptions.Draft => "Draft adverts",
            FilteringOptions.Submitted => "Pending DfE review",
            FilteringOptions.Closed => "Closed adverts",
            FilteringOptions.Live => "Live adverts",
            FilteringOptions.Referred => "Rejected adverts",
            FilteringOptions.NewApplications => "Adverts with new applications",
            FilteringOptions.NewSharedApplications or FilteringOptions.AllSharedApplications => "Adverts with shared applications",
            _ => throw new ArgumentOutOfRangeException(nameof(filteringOption), filteringOption, null)
        };
}
