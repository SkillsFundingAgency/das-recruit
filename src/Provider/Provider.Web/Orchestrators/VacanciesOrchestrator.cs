using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Provider.Web.ViewModels.Vacancies;
using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Shared.Web.ViewModels.Alerts;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Vacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Vacancies;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;

namespace Esfa.Recruit.Provider.Web.Orchestrators;

public class VacanciesOrchestrator(
    IProviderVacancyClient providerVacancyClient,
    IProviderRelationshipsService providerRelationshipsService,
    ITrainingProviderService trainingProviderService,
    IOuterApiClient outerApiClient)
{
    private const int VacanciesPerPage = 25;

    public async Task<VacanciesViewModel> GetVacanciesViewModelAsync(
        VacancyUser user, string filter, int page, string searchTerm)
    {
        long ukprn = user.Ukprn ?? 0;
        var filteringOption = SanitizeFilter(filter);
        var getDashboardTask = providerVacancyClient.GetDashboardAsync(ukprn, user.UserId, page, VacanciesPerPage, "CreatedDate", "Desc", filteringOption, searchTerm);

        var providerTask = providerRelationshipsService.CheckProviderHasPermissions(ukprn, OperationType.RecruitmentRequiresReview);

        await Task.WhenAll(getDashboardTask, providerTask);

        var dashboard = getDashboardTask.Result;
        bool providerPermissions = providerTask.Result;
        int totalItems = Convert.ToInt32(dashboard.TotalVacancies);

        var vacancies = new List<VacancySummary>(dashboard.Vacancies ?? []);
            
        page = SanitizePage(page, totalItems);

        var vacanciesVm = vacancies
            .Select(VacancySummaryMapper.ConvertToVacancySummaryViewModel)
            .ToList();

        var pager = new PagerViewModel(
            totalItems, 
            VacanciesPerPage,
            page, 
            "Showing {0} to {1} of {2} vacancies",
            RouteNames.Vacancies_Get,
            new Dictionary<string, string>
            {
                {"filter", filteringOption.ToString()},
                {"searchTerm", searchTerm}
            });

        var alerts = new AlertsViewModel(new ProviderTransferredVacanciesAlertViewModel
        {
            LegalEntityNames = dashboard.ProviderTransferredVacanciesAlert.LegalEntityNames,
            Ukprn = ukprn
        }, new WithdrawnVacanciesAlertViewModel
        {
            ClosedVacancies = dashboard.WithdrawnVacanciesAlert.ClosedVacancies,
            Ukprn = ukprn
        }, ukprn);

        var vm = new VacanciesViewModel
        {
            Vacancies = vacanciesVm,
            Pager = pager,
            Filter = filteringOption,
            SearchTerm = searchTerm,
            ResultsHeading = VacancyFilterHeadingHelper.GetFilterHeading(Constants.VacancyTerm, totalItems, filteringOption, searchTerm, UserType.Provider),
            Alerts = alerts,
            HasEmployerReviewPermission = providerPermissions,
            Ukprn = ukprn,
            TotalVacancies = totalItems
        };

        return vm;
    }

    private static int SanitizePage(int page, int totalVacancies) 
        => page < 0 || page > (int)Math.Ceiling((double)totalVacancies / VacanciesPerPage) ? 1 : page;

    private static FilteringOptions SanitizeFilter(string filter)
    {
        if (Enum.TryParse(typeof(FilteringOptions), filter, out var status))
            return (FilteringOptions)status;
        return FilteringOptions.Draft;
    }
        
    public async Task<ListVacanciesViewModel> ListVacanciesAsync(
        FilteringOptions filteringOption,
        int ukprn,
        string userId,
        string searchTerm,
        int page,
        int pageSize,
        VacancySortColumn sortColumn,
        ColumnSortOrder sortOrder)
    {
        var alertsTask = trainingProviderService.GetProviderAlerts(ukprn, userId);

        var request = GetVacanciesListRequest(filteringOption, ukprn, searchTerm, page, pageSize, sortColumn, sortOrder);
        var pageHeading = GetPageHeading(filteringOption);
        var result = await outerApiClient.Get<PagedDataResponse<IEnumerable<VacancyListItem>>>(request);
        var totalItems = Convert.ToInt32(result.PageInfo.TotalCount);

        // this is our base route
        var baseRouteDictionary = new Dictionary<string, string> { ["ukprn"] = $"{ukprn}" };
        
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

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            routeDictionary.Add("searchTerm", request.SearchTerm);
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
                ResultsHeading = VacancyFilterHeadingHelper.GetFilterHeading(Constants.VacancyTerm, totalItems, filteringOption, request.SearchTerm, UserType.Provider),
                SearchTerm = request.SearchTerm,
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
                Pagination = new PaginationViewModel(totalItems, request.PageSize, request.Page, "Showing {0} to {1} of {2} vacancies"),
                RouteDictionary = routeDictionary,
                ShowEmployerReviewedApplicationCounts = false,
                ShowSourceOrigin = false,
                SortColumn = request.SortColumn,
                SortOrder = request.SortOrder,
                SubmitVacancyRoute = RouteNames.ProviderCheckYourAnswersGet,
                Vacancies = result.Data.Select(x => VacancyListItemViewModel.From(x, ukprn)).ToList(),
                UserType = UserType.Provider,
            },
            PageHeading = pageHeading,
            Ukprn = ukprn,
        };
    }
    
    private static GetVacanciesByUkprnApiRequestV2 GetVacanciesListRequest(
        FilteringOptions options,
        int ukprn,
        string searchTerm,
        int page,
        int pageSize,
        VacancySortColumn sortColumn,
        ColumnSortOrder sortOrder) =>
        options switch
        {
            FilteringOptions.All => new GetAllVacanciesByUkprnApiRequest(ukprn, searchTerm, page, pageSize, sortColumn, sortOrder),
            FilteringOptions.Draft => new GetDraftVacanciesByUkprnApiRequest(ukprn, searchTerm, page, pageSize, sortColumn, sortOrder),
            FilteringOptions.Review => new GetPendingEmployerReviewedVacanciesByUkprnApiRequest(ukprn, searchTerm, page, pageSize, sortColumn, sortOrder),
            FilteringOptions.Submitted => new GetPendingDfEReviewVacanciesByUkprnApiRequest(ukprn, searchTerm, page, pageSize, sortColumn, sortOrder),
            _ => throw new ArgumentOutOfRangeException(nameof(options), options, null)
        };

    private static string GetPageHeading(FilteringOptions filteringOption) =>
        filteringOption switch
        {
            FilteringOptions.All => "All vacancies",
            FilteringOptions.Draft => "Draft vacancies",
            FilteringOptions.Review => "Pending employer review",
            FilteringOptions.Submitted => "Pending DfE review",
            _ => throw new ArgumentOutOfRangeException(nameof(filteringOption), filteringOption, null)
        };
    }
}