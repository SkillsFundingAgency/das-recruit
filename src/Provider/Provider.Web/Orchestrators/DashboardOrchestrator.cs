using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Microsoft.AspNetCore.Mvc.Rendering;
using NLog.Config;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class DashboardOrchestrator
    {
        private const int VacanciesPerPage = 25;
        private readonly IProviderVacancyClient _client;

        public DashboardOrchestrator(IProviderVacancyClient client)
        {
            _client = client;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(long ukprn, string filter, int page)
        {
            var dashboard = await _client.GetDashboardAsync(ukprn);

            if (dashboard == null)
            {
                await _client.GenerateDashboard(ukprn);
                dashboard = await _client.GetDashboardAsync(ukprn);
            }

            var filterStatus = SanitizeFilter(filter);

            var filterOptions = new List<SelectListItem>
            {
                GetFilterSelectListItem(dashboard.Vacancies, "All vacancies", null, filterStatus),
                GetFilterSelectListItem(dashboard.Vacancies, "Live vacancies", VacancyStatus.Live, filterStatus),
                GetFilterSelectListItem(dashboard.Vacancies, "Rejected vacancies", VacancyStatus.Referred, filterStatus),
                GetFilterSelectListItem(dashboard.Vacancies, "Pending vacancies", VacancyStatus.Submitted, filterStatus),
                GetFilterSelectListItem(dashboard.Vacancies, "Draft vacancies", VacancyStatus.Draft, filterStatus),
                GetFilterSelectListItem(dashboard.Vacancies, "Closed vacancies", VacancyStatus.Closed, filterStatus),
            };

            var filteredVacancies = dashboard?.Vacancies
                .Where(v => filterStatus.HasValue == false || v.Status == filterStatus.Value)
                .ToList();

            var totalVacancies = filteredVacancies.Count();

            page = SanitizePage(page, totalVacancies);

            var skip = (page - 1) * VacanciesPerPage;

            var vacancies = filteredVacancies
                .Skip(skip)
                .Take(VacanciesPerPage)
                .Select(VacancySummaryMapper.ConvertToVacancySummaryViewModel)
                .OrderByDescending(v => v.CreatedDate)
                .ToList();

            var pager = new PagerViewModel(
                totalVacancies, 
                VacanciesPerPage,
                page, 
                "Showing {0} to {1} of {2} vacancies",
                RouteNames.Dashboard_Index_Get,
                new Dictionary<string, string>
                {
                    {"filter", filterStatus?.ToString() ?? ""}
                });

            var vm = new DashboardViewModel 
            {
                Vacancies = vacancies,
                Pager = pager,
                FilterOptions = filterOptions
            };

            return vm;
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

        private SelectListItem GetFilterSelectListItem(IEnumerable<VacancySummary> vacancies, string text, VacancyStatus? optionStatus, VacancyStatus? filterStatus)
        {
            var count = vacancies.Count(v => 
                optionStatus.HasValue == false || 
                v.Status == optionStatus.Value);

            var value = optionStatus.HasValue ? optionStatus.Value.ToString() : "";

            return new SelectListItem($"{text} ({count})", value, optionStatus == filterStatus );
        }
    }
}