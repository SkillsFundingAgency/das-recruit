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
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
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
    private const int MinPage = 1;
    private const int MaxPage = 9999;
    private static int ClampPage(int page) => Math.Clamp(page, MinPage, MaxPage);
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

    private static int SanitizePage(int page, int totalVacancies)
    {
        return (page < 0 || page > (int)Math.Ceiling((double)totalVacancies / VacanciesPerPage)) ? 1 : page;
    }

    private static FilteringOptions SanitizeFilter(string filter)
    {
        if (Enum.TryParse(typeof(FilteringOptions), filter, out var status))
            return (FilteringOptions)status;
        return FilteringOptions.All;
    }

    private static Dictionary<string, string> GetRouteDictionary(string employerAccountId, string searchTerm, VacancySortColumn? sortColumn, ColumnSortOrder? sortOrder)
    {
        var result = new Dictionary<string, string> { ["employerAccountId"] = $"{employerAccountId}" };
        if (sortColumn is not (null or VacancySortColumn.CreatedDate)) // ignore default
        {
            result.Add("sortColumn", $"{sortColumn}");
            if (sortOrder is not null)
            {
                // only order if the sort column is set
                result.Add("sortOrder", $"{sortOrder}");
            }
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            result.Add("searchTerm", searchTerm);
        }

        return result;
    }
        
    public async Task<ListAllVacanciesViewModel> ListAllVacanciesAsync(
        string employerAccountId,
        long? ukprn,
        string userId,
        int? page,
        int pageSize,
        string searchTerm,
        VacancySortColumn? sortColumn,
        ColumnSortOrder? sortOrder)
    {
        page = ClampPage(page ?? 1);

        var resultTask = outerApiClient.Get<PagedDataResponse<IEnumerable<VacancyListItem>>>(
            new GetVacanciesByEmployerAccountApiRequestV2(
                encodingService.Decode(employerAccountId, EncodingType.AccountId),
                searchTerm,
                page.Value,
                pageSize,
                sortColumn ?? VacancySortColumn.CreatedDate,
                sortOrder ?? ColumnSortOrder.Desc)
        );
        var alertsTask = employerAccountProvider.GetEmployerAlerts(employerAccountId, userId);
        await Task.WhenAll(resultTask, alertsTask);
        var result = resultTask.Result;
        var alerts = alertsTask.Result;
        var totalItems = Convert.ToInt32(result.PageInfo.TotalCount);

        var routeDictionary = GetRouteDictionary(employerAccountId, searchTerm, sortColumn, sortOrder);
        
        return new ListAllVacanciesViewModel
        {
            Alerts = new AlertsViewModel(
                new EmployerTransferredVacanciesAlertViewModel
                {
                    EmployerAccountId = employerAccountId,
                    TransferredVacanciesCount = alerts.EmployerRevokedTransferredVacanciesAlert.TransferredVacanciesCount,
                    TransferredVacanciesProviderNames = alerts.EmployerRevokedTransferredVacanciesAlert.TransferredVacanciesProviderNames,

                },
                new EmployerTransferredVacanciesAlertViewModel
                {
                    EmployerAccountId = employerAccountId,
                    TransferredVacanciesCount = alerts.BlockedProviderTransferredVacanciesAlert.TransferredVacanciesCount,
                    TransferredVacanciesProviderNames = alerts.BlockedProviderTransferredVacanciesAlert.TransferredVacanciesProviderNames
                },
                new BlockedProviderAlertViewModel
                {
                    EmployerAccountId = employerAccountId,
                    BlockedProviderNames = alerts.BlockedProviderAlert.BlockedProviderNames,
                    ClosedVacancies = alerts.BlockedProviderAlert.ClosedVacancies,
                },
                new WithdrawnVacanciesAlertViewModel
                {
                    EmployerAccountId = employerAccountId,
                    ClosedVacancies = alerts.WithDrawnByQaVacanciesAlert.ClosedVacancies,
                    Ukprn = ukprn.GetValueOrDefault()
                }
            ),
            EmployerAccountId = employerAccountId,
            ListViewModel = new VacanciesListViewModel
            {
                EditVacancyRoute = RouteNames.EmployerTaskListGet,
                ManageVacancyRoute = RouteNames.VacancyManage_Get,
                Pagination = new PaginationViewModel(totalItems, pageSize, page.Value, "Showing {0} to {1} of {2} adverts"),
                RouteDictionary = routeDictionary,
                ShowEmployerReviewedApplicationCounts = false,
                ShowSourceOrigin = false,
                SortColumn = sortColumn,
                SortOrder = sortOrder,
                SubmitVacancyRoute = RouteNames.EmployerCheckYourAnswersGet,
                Vacancies = result.Data.Select(x => VacancyListItemViewModel.From(x, employerAccountId)).ToList(),
                UserType = UserType.Employer,
            },
            ResultsHeading = VacancyFilterHeadingHelper.GetFilterHeading(Constants.VacancyTerm, totalItems, FilteringOptions.All, searchTerm, UserType.Provider),
            SearchTerm = searchTerm,
            TotalVacancies = (uint)totalItems,
        };
    }
}