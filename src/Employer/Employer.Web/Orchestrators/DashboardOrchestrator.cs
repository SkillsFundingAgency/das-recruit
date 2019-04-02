using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Humanizer;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class DashboardOrchestrator
    {
        private const int VacanciesPerPage = 25;
        private readonly IEmployerVacancyClient _vacancyClient;

        public DashboardOrchestrator(IEmployerVacancyClient vacancyClient)
        {
            _vacancyClient = vacancyClient;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(string employerAccountId, string filter, int page)
        {
            var vacancies = await GetVacanciesAsync(employerAccountId);

            var filterStatus = SanitizeFilter(filter);

            var filteredVacancies = vacancies.Where(v =>
                    filterStatus.HasValue == false || v.Status == filterStatus.Value)
                .OrderByDescending(v => v.CreatedDate)
                .ToList();

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
                    {"filter", filterStatus?.ToString() ?? ""}
                });

            var vm = new DashboardViewModel {
                Vacancies = vacanciesVm,
                Pager = pager,
                IsFiltered = filterStatus.HasValue,
                ShowFilter = vacancies.Select(v => v.Status).Distinct().Count() > 1,
                FilterOptions = GetFilterSelectOptions(vacancies, filterStatus),
                ResultsHeading = GetFilterHeading(filteredVacanciesTotal, filterStatus),
                HasVacancies = vacancies.Any()
            };

            return vm;
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

        private VacancyStatus? SanitizeFilter(string filter)
        {
            if (Enum.TryParse(typeof(VacancyStatus), filter, out var status))
                return (VacancyStatus)status;

            return null;
        }

        private List<SelectListItem> GetFilterSelectOptions(List<VacancySummary> vacancies, VacancyStatus? filterStatus)
        {
            return new List<SelectListItem>
            {
                GetFilterSelectListItem(vacancies, null, filterStatus),
                GetFilterSelectListItem(vacancies, VacancyStatus.Draft, filterStatus),
                GetFilterSelectListItem(vacancies, VacancyStatus.Submitted, filterStatus),
                GetFilterSelectListItem(vacancies, VacancyStatus.Live, filterStatus),
                GetFilterSelectListItem(vacancies, VacancyStatus.Closed, filterStatus),
                GetFilterSelectListItem(vacancies, VacancyStatus.Referred, filterStatus),
            };
        }

        private SelectListItem GetFilterSelectListItem(IEnumerable<VacancySummary> vacancies, VacancyStatus? optionStatus, VacancyStatus? filterStatus)
        {
            var count = vacancies.Count(v =>
                optionStatus.HasValue == false ||
                v.Status == optionStatus.Value);

            var value = optionStatus.HasValue ? optionStatus.Value.ToString() : "All";

            var text = optionStatus.HasValue ? optionStatus.Value.GetDisplayName() : "All Vacancies";

            return new SelectListItem($"{text} ({count})", value, optionStatus == filterStatus);
        }

        private string GetFilterHeading(int totalVacancies, VacancyStatus? filterStatus)
        {
            if (totalVacancies == 1 && filterStatus.HasValue == false)
                return "Showing 1 vacancy";

            var filterText = filterStatus.HasValue ? filterStatus.GetDisplayName() : "All";

            if (filterStatus.HasValue)
                return $"Showing {totalVacancies} \"{filterText}\" {"vacancy".ToQuantity(totalVacancies, ShowQuantityAs.None)}";

            return $"Showing all {totalVacancies} vacancies";
        }
    }
}