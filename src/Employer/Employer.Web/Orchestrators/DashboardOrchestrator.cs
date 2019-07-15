using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.ViewModels.Dashboard;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Mappers;
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

        public DashboardOrchestrator(IEmployerVacancyClient vacancyClient, ITimeProvider timeProvider, IRecruitVacancyClient client)
        {
            _vacancyClient = vacancyClient;
            _timeProvider = timeProvider;
            _client = client;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(string employerAccountId, string filter, int page, VacancyUser user)
        {
            var vacanciesTask = GetVacanciesAsync(employerAccountId);
            var userDetailsTask = _client.GetUsersDetailsAsync(user.UserId);

            await Task.WhenAll(vacanciesTask, userDetailsTask);

            var vacancies = vacanciesTask.Result;
            var userDetails = userDetailsTask.Result;

            var filteringOption = SanitizeFilter(filter);

            var filteredVacancies = GetFilteredVacancies(vacancies, filteringOption);

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
                ResultsHeading = GetFilterHeading(filteredVacanciesTotal, filteringOption),
                HasVacancies = vacancies.Any(),
                TransferredVacanciesAlert = GetTransferredVacanciesAlertViewModel(vacancies, userDetails.TransferredVacanciesAlertDismissedOn)
            };

            return vm;
        }

        public Task DismissAlert(VacancyUser user, AlertType alertType)
        {
            return _client.UpdateUserAlertAsync(user.UserId, alertType, _timeProvider.Now);
        }

        private List<VacancySummary> GetFilteredVacancies(List<VacancySummary> vacancies, FilteringOptions filterStatus)
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
            return filteredVacancies.OrderByDescending(v => v.CreatedDate)
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

        private string GetFilterHeading(int totalVacancies, FilteringOptions filteringOption)
        {
            var filterText = filteringOption.GetDisplayName().ToLowerInvariant();
            switch (filteringOption)
            {
                case FilteringOptions.ClosingSoon:
                case FilteringOptions.ClosingSoonWithNoApplications:
                    return $"{totalVacancies} {"live vacancy".ToQuantity(totalVacancies, ShowQuantityAs.None)} {filterText}";
                case FilteringOptions.AllApplications:
                case FilteringOptions.NewApplications:
                    return $"{totalVacancies} {"vacancy".ToQuantity(totalVacancies, ShowQuantityAs.None)} {filterText}";
                case FilteringOptions.All:
                    return $"All {totalVacancies} vacancies";
                default:
                    return $"{totalVacancies} {filterText} {"vacancy".ToQuantity(totalVacancies, ShowQuantityAs.None)}";
            }
        }

        private TransferredVacanciesAlertViewModel GetTransferredVacanciesAlertViewModel(IEnumerable<VacancySummary> vacancies, DateTime? userLastDismissedDate)
        {
            if (userLastDismissedDate.HasValue == false)
                userLastDismissedDate = DateTime.MinValue;

            var transferredVacancyProviders = vacancies.Where(v =>
                    v.TransferInfoReason == TransferReason.EmployerRevokedPermission &&
                    v.TransferInfoTransferredDate > userLastDismissedDate)
                .Select(v => v.TransferInfoProviderName).ToList();

            if (transferredVacancyProviders.Any() == false)
                return null;
            
            return new TransferredVacanciesAlertViewModel
            {
                TransferredVacanciesCount = transferredVacancyProviders.Count,
                TransferredVacanciesProviderNames = transferredVacancyProviders.GroupBy(p => p).Select(p => p.Key)
            };
        }
    }
}