using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Employer.Web.ViewModels.Vacancies;
using Esfa.Recruit.Employer.Web.ViewModels.Alerts;
using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Vacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Vacancies;
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
        string searchTerm,
        int page,
        int pageSize,
        VacancySortColumn sortColumn,
        ColumnSortOrder sortOrder)
    {
        var alertsTask = employerAccountProvider.GetEmployerAlerts(hashedEmployerAccountId, userId);
        
        var request = GetVacanciesListRequest(filteringOption, hashedEmployerAccountId, searchTerm, page, pageSize, sortColumn, sortOrder);
        var pageHeading = GetPageHeading(filteringOption);
        var result = await outerApiClient.Get<PagedDataResponse<IEnumerable<VacancyListItem>>>(request);
        var totalItems = Convert.ToInt32(result.PageInfo.TotalCount);

        // this is our base route
        var baseRouteDictionary = new Dictionary<string, string> { ["employerAccountId"] = hashedEmployerAccountId };
        
        // create a separate dict with search params included
        var routeDictionary = new Dictionary<string, string>(baseRouteDictionary);
        if (request.SortColumn is not (null or VacancySortColumn.CreatedDate)) // ignore default
        {
            routeDictionary.Add("sortColumn", $"{request.SortColumn}");
            if (request.SortOrder is not null)
            {
                // only order if the sort column is set
                routeDictionary.Add("sortOrder", $"{request.SortOrder}");
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
                ResultsHeading = VacancyFilterHeadingHelper.GetFilterHeading(Constants.VacancyTerm, totalItems, filteringOption, request.SearchTerm, UserType.Employer),
                SearchTerm = request.SearchTerm,
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
                ShowApplicationsColumn = ShouldShowApplicationsColumn(filteringOption),
                ShowEmployerReviewedApplicationCounts = false,
                ShowSourceOrigin = false,
                SortColumn = request.SortColumn,
                SortOrder = request.SortOrder,
                SubmitVacancyRoute = RouteNames.EmployerCheckYourAnswersGet,
                Vacancies = result.Data.Select(x => VacancyListItemViewModel.From(x, hashedEmployerAccountId)).ToList(),
                UserType = UserType.Employer,
            },
            PageHeading = pageHeading,
        };
    }
    
    private GetVacanciesByEmployerAccountApiRequestV2 GetVacanciesListRequest(
        FilteringOptions options,
        string hashedEmployerAccountId,
        string searchTerm,
        int page,
        int pageSize,
        VacancySortColumn sortColumn,
        ColumnSortOrder sortOrder)
    {
        var employerAccountId = encodingService.Decode(hashedEmployerAccountId, EncodingType.AccountId);
        return options switch
        {
            FilteringOptions.All => new GetAllVacanciesByEmployerAccountApiRequest(employerAccountId, searchTerm, page, pageSize, sortColumn, sortOrder),
            FilteringOptions.Draft => new GetDraftVacanciesByEmployerAccountApiRequest(employerAccountId, searchTerm, page, pageSize, sortColumn, sortOrder),
            FilteringOptions.Submitted => new GetPendingDfEReviewVacanciesByEmployerAccountApiRequest(employerAccountId, searchTerm, page, pageSize, sortColumn, sortOrder),
            _ => throw new ArgumentOutOfRangeException(nameof(options), options, null)
        };
    }

    private static string GetPageHeading(FilteringOptions filteringOption) =>
        filteringOption switch
        {
            FilteringOptions.All => "All adverts",
            FilteringOptions.Draft => "Draft adverts",
            FilteringOptions.Submitted => "Pending DfE review",
            _ => throw new ArgumentOutOfRangeException(nameof(filteringOption), filteringOption, null)
        };

    private static bool ShouldShowApplicationsColumn(FilteringOptions filteringOption) =>
        filteringOption switch
        {
            FilteringOptions.All => true,
            FilteringOptions.Live => true,
            FilteringOptions.Closed => true,
            _ => false
        };
}