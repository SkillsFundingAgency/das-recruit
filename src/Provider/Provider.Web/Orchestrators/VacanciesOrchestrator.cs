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
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Vacancies;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class VacanciesOrchestrator(
        IProviderVacancyClient providerVacancyClient,
        IProviderRelationshipsService providerRelationshipsService,
        ITrainingProviderService trainingProviderService,
        IOuterApiClient outerApiClient)
    {
        private const int MinPage = 1;
        private const int MaxPage = 9999;
        private static int ClampPage(int page) => Math.Clamp(page, MinPage, MaxPage);
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

        private int SanitizePage(int page, int totalVacancies)
        {
            return (page < 0 || page > (int)Math.Ceiling((double)totalVacancies / VacanciesPerPage)) ? 1 : page;
        }

        private FilteringOptions SanitizeFilter(string filter)
        {
            if (Enum.TryParse(typeof(FilteringOptions), filter, out var status))
                return (FilteringOptions)status;
            return FilteringOptions.All;
        }
        
        public async Task<ListAllVacanciesViewModel> ListAllVacanciesAsync(
            int ukprn,
            string userId,
            int? page,
            int pageSize,
            string searchTerm,
            VacancySortColumn? sortColumn,
            ColumnSortOrder? sortOrder)
        {
            page = ClampPage(page ?? 1);
            
            var resultTask = outerApiClient.Get<PagedDataResponse<IEnumerable<VacancyListItem>>>(
                new GetVacanciesByUkprnApiRequestV2(
                    ukprn,
                    searchTerm,
                    page.Value,
                    pageSize,
                    sortColumn ?? VacancySortColumn.CreatedDate,
                    sortOrder ?? ColumnSortOrder.Desc)
            );
            var alertsTask = trainingProviderService.GetProviderAlerts(ukprn, userId);
            await Task.WhenAll(resultTask, alertsTask);
            var result = resultTask.Result;
            var alerts = alertsTask.Result;
            var totalItems = Convert.ToInt32(result.PageInfo.TotalCount);

            var routeDictionary = new Dictionary<string, string> { ["ukprn"] = $"{ukprn}" };
            if (sortColumn is not (null or VacancySortColumn.CreatedDate)) // ignore default
            {
                routeDictionary.Add("sortColumn", $"{sortColumn}");
                if (sortOrder is not null)
                {
                    // only order if the sort column is set
                    routeDictionary.Add("sortOrder", $"{sortOrder}");
                }
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                routeDictionary.Add("searchTerm", searchTerm);
            }
            
            return new ListAllVacanciesViewModel
            {
                Alerts = new AlertsViewModel(null,
                    new WithdrawnVacanciesAlertViewModel
                    {
                        ClosedVacancies = alerts.WithdrawnVacanciesAlert.ClosedVacancies,
                        Ukprn = ukprn
                    },
                    ukprn
                ),
                ListViewModel = new VacanciesListViewModel
                {
                    EditVacancyRoute = RouteNames.ProviderTaskListGet,
                    ManageVacancyRoute = RouteNames.VacancyManage_Get,
                    Pagination = new PaginationViewModel(totalItems, pageSize, page.Value, "Showing {0} to {1} of {2} vacancies"),
                    RouteDictionary = routeDictionary,
                    ShowEmployerReviewedApplicationCounts = false,
                    ShowSourceOrigin = false,
                    SortColumn = sortColumn,
                    SortOrder = sortOrder,
                    SubmitVacancyRoute = RouteNames.ProviderCheckYourAnswersGet,
                    Vacancies = result.Data.Select(x => VacancyListItemViewModel.From(x, ukprn)).ToList(),
                    ViewType = OwnerType.Provider,
                },
                ResultsHeading = VacancyFilterHeadingHelper.GetFilterHeading(Constants.VacancyTerm, totalItems, FilteringOptions.All, searchTerm, UserType.Provider),
                SearchTerm = searchTerm,
                TotalVacancies = (uint)totalItems,
                Ukprn = ukprn,
            };
        }
    }
}