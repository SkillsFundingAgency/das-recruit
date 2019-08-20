﻿using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.ViewModels.Dashboard;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Humanizer;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class DashboardOrchestrator
    {
        private const int VacanciesPerPage = 25;
        private readonly ITimeProvider _timeProvider;
        private const int ClosingSoonDays = 5;
        private readonly IEmployerVacancyClient _vacancyClient;
        private readonly IRecruitVacancyClient _client;
        private readonly AlertViewModelService _alertViewModelService;

        public DashboardOrchestrator(IEmployerVacancyClient vacancyClient, ITimeProvider timeProvider, IRecruitVacancyClient client, AlertViewModelService alertViewModelService)
        {
            _vacancyClient = vacancyClient;
            _timeProvider = timeProvider;
            _client = client;
            _alertViewModelService = alertViewModelService;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(string employerAccountId, string filter, int page, VacancyUser user, string searchTerm)
        {
            var vacanciesTask = GetVacanciesAsync(employerAccountId);
            var userDetailsTask = _client.GetUsersDetailsAsync(user.UserId);

            await Task.WhenAll(vacanciesTask, userDetailsTask);

            var vacancies = vacanciesTask.Result;
            var userDetails = userDetailsTask.Result;

            var filteringOption = SanitizeFilter(filter);

            var filteredVacancies = GetFilteredVacancies(vacancies, filteringOption, searchTerm);

            var filteredVacanciesTotal = filteredVacancies.Count();

            page = SanitizePage(page, filteredVacanciesTotal);

            var skip = (page - 1) * VacanciesPerPage;

            var vacanciesVm = filteredVacancies
                .Skip(skip)
                .Take(VacanciesPerPage)
                .Select(VacancySummaryMapper.ConvertToVacancySummaryViewModel)
                .ToList();

            var pager = new PagerViewModel(
                filteredVacanciesTotal,
                VacanciesPerPage,
                page,
                "Showing {0} to {1} of {2} vacancies",
                RouteNames.Dashboard_Index_Get,
                new Dictionary<string, string>
                {
                    {"filter", filteringOption.ToString()}
                });
            
            var vm = new DashboardViewModel {
                Vacancies = vacanciesVm,
                Pager = pager,
                Filter = filteringOption,
                ResultsHeading = GetFilterHeading(filteredVacanciesTotal, filteringOption, searchTerm),
                HasVacancies = vacancies.Any(),
                Alerts = GetAlerts(vacancies, userDetails)
            };

            return vm;
        }

        public Task DismissAlert(VacancyUser user, AlertType alertType)
        {
            return _client.UpdateUserAlertAsync(user.UserId, alertType, _timeProvider.Now);
        }

        private List<VacancySummary> GetFilteredVacancies(List<VacancySummary> vacancies, FilteringOptions filterStatus, string searchTerm)
        {
            IEnumerable<VacancySummary> filteredVacancies = new List<VacancySummary>();
            switch (filterStatus)
            {
                case FilteringOptions.Live:
                case FilteringOptions.Closed:
                case FilteringOptions.Referred:
                case FilteringOptions.Draft:
                case FilteringOptions.Submitted:
                    filteredVacancies = vacancies.Where(v =>
                        v.Status.ToString() == filterStatus.ToString());
                    break;
                case FilteringOptions.All:
                    filteredVacancies = vacancies;
                    break;
                case FilteringOptions.NewApplications:
                    filteredVacancies = vacancies.Where(v => v.NoOfNewApplications > 0);
                    break;
                case FilteringOptions.AllApplications:
                    filteredVacancies = vacancies.Where(v => v.NoOfApplications > 0);
                    break;
                case FilteringOptions.ClosingSoon:
                    filteredVacancies = vacancies.Where(v =>
                        v.ClosingDate <= _timeProvider.Today.AddDays(ClosingSoonDays) &&
                        v.Status == VacancyStatus.Live);
                    break;
                case FilteringOptions.ClosingSoonWithNoApplications:
                    filteredVacancies = vacancies.Where(v =>
                        v.ClosingDate <= _timeProvider.Today.AddDays(ClosingSoonDays) &&
                        v.Status == VacancyStatus.Live &&
                        v.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship &&
                        v.NoOfApplications == 0);
                    break;
                case FilteringOptions.Transferred:
                        filteredVacancies = vacancies.Where(v => v.TransferInfoTransferredDate.HasValue);
                    break;
            }
            return filteredVacancies
                .Where(v => string.IsNullOrWhiteSpace(searchTerm)
                            || (v.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            || (string.IsNullOrWhiteSpace(v.LegalEntityName) == false && v.LegalEntityName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            || (v.VacancyReference.HasValue && $"VAC{v.VacancyReference}".Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(v => v.CreatedDate)
                .ToList();
        }

        private async Task<List<VacancySummary>> GetVacanciesAsync(string employerAccountId)
        {
            var dashboard = await _vacancyClient.GetDashboardAsync(employerAccountId);

            if (dashboard == null)
            {
                await _vacancyClient.GenerateDashboard(employerAccountId);
                dashboard = await _vacancyClient.GetDashboardAsync(employerAccountId);
            }

            return dashboard?.Vacancies?.ToList() ?? new List<VacancySummary>();
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

        private string GetFilterHeading(int totalVacancies, FilteringOptions filteringOption, string searchTerm)
        {
            var filterText = filteringOption == FilteringOptions.All ? string.Empty : $" {filteringOption.GetDisplayName().ToLowerInvariant()}";
            var vacancyText = filteringOption == FilteringOptions.ClosingSoon || filteringOption == FilteringOptions.ClosingSoonWithNoApplications ?
                " live vacancy" : " vacancy";
            var vacancyStatusPrefix = $"{totalVacancies}{filterText}{vacancyText}".ToQuantity(totalVacancies, ShowQuantityAs.None);

            var searchSuffix = string.IsNullOrWhiteSpace(searchTerm) ? string.Empty : $" with '{searchTerm}'";

            return $"{vacancyStatusPrefix}{searchSuffix}";
        }

        private AlertsViewModel GetAlerts(IEnumerable<VacancySummary> vacancies, User userDetails)
        {
            return new AlertsViewModel
            {
                EmployerRevokedTransferredVacanciesAlert = _alertViewModelService.GetTransferredVacanciesAlert(vacancies, TransferReason.EmployerRevokedPermission, userDetails.TransferredVacanciesEmployerRevokedPermissionAlertDismissedOn),
                BlockedProviderTransferredVacanciesAlert = _alertViewModelService.GetTransferredVacanciesAlert(vacancies, TransferReason.BlockedByQa, userDetails.TransferredVacanciesBlockedProviderAlertDismissedOn),
                BlockedProviderAlert = _alertViewModelService.GetBlockedProviderVacanciesAlert(vacancies, userDetails.ClosedVacanciesBlockedProviderAlertDismissedOn),
                WithdrawnByQaVacanciesAlert = _alertViewModelService.GetWithdrawnByQaVacanciesAlert(vacancies, userDetails.ClosedVacanciesWithdrawnByQaAlertDismissedOn)
            };
        }
    }
}