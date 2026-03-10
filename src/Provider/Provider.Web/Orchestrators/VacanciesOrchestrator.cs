#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Provider.Web.ViewModels.Vacancies;
using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Shared.Web.ViewModels.Alerts;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Vacancy.Provider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Vacancies;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;

namespace Esfa.Recruit.Provider.Web.Orchestrators;

public class VacanciesOrchestrator(
    ITrainingProviderService trainingProviderService,
    IOuterApiClient outerApiClient)
{
    private const int VacanciesPerPage = 25;
    
    public async Task<ListVacanciesViewModel> ListVacanciesAsync(
        FilteringOptions filteringOption,
        int ukprn,
        string userId,
        string? searchTerm = null,
        int page = 1,
        int pageSize = VacanciesPerPage,
        VacancySortColumn sortColumn = VacancySortColumn.CreatedDate,
        ColumnSortOrder sortOrder = ColumnSortOrder.Desc)
    {
        var alertsTask = trainingProviderService.GetProviderAlerts(ukprn, userId);

        var pageHeading = GetPageHeading(filteringOption);
        var result = await GetVacancies(filteringOption, ukprn, searchTerm, page, pageSize, sortColumn, sortOrder);
        var totalItems = Convert.ToInt32(result.PageInfo.TotalCount);
        
        // this is our base route
        var baseRouteDictionary = new Dictionary<string, string> { ["ukprn"] = $"{ukprn}" };

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

        var alerts = alertsTask.Result;
        return new ListVacanciesViewModel
        {
            Alerts = new AlertsViewModel(null,
                new WithdrawnVacanciesAlertViewModel
                {
                    ClosedVacancies = alerts.WithdrawnVacanciesAlert.ClosedVacancies,
                    Ukprn = ukprn
                },
                ukprn
            ),
            FilterViewModel = new VacanciesListSearchFilterViewModel
            {
                ResultsHeading = VacancyFilterHeadingHelper.GetFilterHeading(Constants.VacancyTerm, totalItems, filteringOption, searchTerm, UserType.Provider),
                SearchTerm = searchTerm,
                SuggestionsEnabled = false, // TODO: disable for the moment it doesn't take into account the vacancy status, so would suggest things not in the list
                SuggestionsRoute = RouteNames.VacanciesSearchSuggestions_Get,
                SuggestionsRouteDictionary = routeDictionary,
                RouteDictionary = baseRouteDictionary,
                UserType = UserType.Provider,
            },
            ListViewModel = new VacanciesListViewModel
            {
                EditVacancyRoute = RouteNames.ProviderTaskListGet,
                ManageVacancyRoute = RouteNames.VacancyManage_Get,
                Pagination = new PaginationViewModel(totalItems, pageSize, page, "Showing {0} to {1} of {2} vacancies"),
                RouteDictionary = routeDictionary,
                ShowEmployerReviewedApplicationCounts = filteringOption == FilteringOptions.EmployerReviewedApplications,
                ShowSourceOrigin = true,
                SortColumn = sortColumn,
                SortOrder = sortOrder,
                SubmitVacancyRoute = RouteNames.ProviderCheckYourAnswersGet,
                Vacancies = result.Data.Select(x => VacancyListItemViewModel.From(x, ukprn, filteringOption)).ToList(),
                UserType = UserType.Provider,
            },
            PageHeading = pageHeading,
            Ukprn = ukprn,
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
        int ukprn,
        string searchTerm,
        int page,
        int pageSize,
        VacancySortColumn sortColumn,
        ColumnSortOrder sortOrder)
    {
        if (!Enum.IsDefined(typeof(FilteringOptions), options))
            throw new ArgumentOutOfRangeException(nameof(options), options, null);

        return ApplicationListOptions.Contains(options)
            ? await GetVacanciesListIncludingApplicationsAsync(options, ukprn, searchTerm, page, pageSize, sortColumn, sortOrder)
            : await GetVacanciesList(options, ukprn, searchTerm, page, pageSize, sortColumn, sortOrder);
    }

    private async Task<PagedDataResponse<IEnumerable<VacancyListItem>>> GetVacanciesList(FilteringOptions options,
        int ukprn,
        string searchTerm,
        int page,
        int pageSize,
        VacancySortColumn sortColumn,
        ColumnSortOrder sortOrder)
    {
        var request = GetVacanciesListRequest(options, ukprn, searchTerm, page, pageSize, sortColumn, sortOrder);
        return await outerApiClient.Get<PagedDataResponse<IEnumerable<VacancyListItem>>>(request);
    }

    private async Task<PagedDataResponse<IEnumerable<VacancyListItem>>> GetVacanciesListIncludingApplicationsAsync(FilteringOptions options,
        int ukprn,
        string searchTerm,
        int page,
        int pageSize,
        VacancySortColumn sortColumn,
        ColumnSortOrder sortOrder)
    {
        var request = new GetVacanciesByUkprnApiRequest(ukprn,
            page,
            pageSize,
            sortColumn.ToString(),
            sortOrder.ToString(),
            options,
            searchTerm);

        var response = await outerApiClient.Get<GetVacanciesByUkprnApiResponse>(request);

        var items = response.VacancySummaries.Select(x => (VacancyListItem)x).ToList();

        return new PagedDataResponse<IEnumerable<VacancyListItem>>(items,
            new PageInfo(
                (ushort)page,
                (ushort)pageSize,
                (uint)response.PageInfo.TotalCount));
    }

    private static GetVacanciesByUkprnAndStatusApiRequest GetVacanciesListRequest(FilteringOptions options,
        int ukprn,
        string searchTerm,
        int page,
        int pageSize,
        VacancySortColumn sortColumn,
        ColumnSortOrder sortOrder)
    {
        if (!Enum.IsDefined(typeof(FilteringOptions), options))
            throw new ArgumentOutOfRangeException(nameof(options), options, null);

        return new GetVacanciesByUkprnAndStatusApiRequest(
            ukprn, searchTerm, page, pageSize, options, sortColumn, sortOrder);
    }

    private static string GetPageHeading(FilteringOptions filteringOption) =>
        filteringOption switch
        {
            FilteringOptions.All => "All vacancies",
            FilteringOptions.Draft => "Draft vacancies",
            FilteringOptions.Review => "Pending employer review",
            FilteringOptions.Submitted => "Pending DfE review",
            FilteringOptions.Live => "Live vacancies",
            FilteringOptions.Closed => "Closed vacancies",
            FilteringOptions.Referred => "Rejected vacancies",
            FilteringOptions.AllApplications => "Vacancies with applications",
            FilteringOptions.NewApplications => "Vacancies with new applications",
            FilteringOptions.EmployerReviewedApplications => "Employer reviewed applications",
            _ => "Vacancies"
        };
}