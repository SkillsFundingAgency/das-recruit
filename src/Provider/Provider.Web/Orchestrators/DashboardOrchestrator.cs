using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Humanizer;

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
            var vacancies = await GetVacanciesAsync(ukprn);

            var filterStatus = SanitizeFilter(filter);

            var filteredVacancies = GetFilteredVacancies(vacancies, filterStatus);                
            
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
            
            var vm = new DashboardViewModel 
            {
                Vacancies = vacanciesVm,
                Pager = pager,
                IsFiltered = filterStatus.HasValue,                 
                Filter = filterStatus?.ToString(),
                ResultsHeading = GetFilterHeading(filteredVacanciesTotal, filterStatus),
                HasVacancies = vacancies.Any()
            };

            return vm;
        }

        private List<VacancySummary> GetFilteredVacancies(List<VacancySummary> vacancies, FilteringOptions? filterStatus)
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
                        v.Status == (VacancyStatus) filterStatus.Value);                        
                    break;
                case FilteringOptions.All:
                    filteredVacancies = vacancies;
                    break;
                case FilteringOptions.NewApplications:
                    filteredVacancies = vacancies.Where(v => v.NoOfNewApplications > 0);
                    break;
                case FilteringOptions.AllApplications:
                    filteredVacancies = vacancies.Where(v =>
                        v.NoOfSuccessfulApplications > 0 || v.NoOfUnsuccessfulApplications > 0 ||
                        v.NoOfNewApplications > 0);
                    break;
                case FilteringOptions.ClosingSoon:
                    filteredVacancies = vacancies.Where(v =>
                        v.ClosingDate <= DateTime.Now.AddDays(5) && v.Status == VacancyStatus.Live);
                    break;
                case FilteringOptions.ClosingSoonWithNoApplications:
                    filteredVacancies = vacancies.Where(v =>
                        v.ClosingDate <= DateTime.Now.AddDays(5) && v.Status == VacancyStatus.Live && (v.NoOfSuccessfulApplications == 0 || v.NoOfUnsuccessfulApplications == 0 ||
                                                                     v.NoOfNewApplications == 0));
                    break;
            }
            return filteredVacancies.OrderByDescending(v => v.CreatedDate)
                .ToList(); 
        }

        private async Task<List<VacancySummary>> GetVacanciesAsync(long ukprn)
        {
            var dashboard = await _client.GetDashboardAsync(ukprn);

            if (dashboard == null)
            {
                await _client.GenerateDashboard(ukprn);
                dashboard = await _client.GetDashboardAsync(ukprn);
            }

            return dashboard?.Vacancies?.ToList() ?? new List<VacancySummary>();
        }

        private int SanitizePage(int page, int totalVacancies)
        {
            return page < 0 || page > (int)Math.Ceiling((double)totalVacancies / VacanciesPerPage) ? 1 : page;
        }

        private FilteringOptions? SanitizeFilter(string filter)
        {
            if (Enum.TryParse(typeof(FilteringOptions), filter, out var status))
                return (FilteringOptions)status;

            return null;
        }

        private string GetFilterHeading(int totalVacancies, FilteringOptions? filterStatus)
        {
            if (totalVacancies == 1 && filterStatus.HasValue == false)
                return "Show 1 vacancy";
            var filterText = filterStatus.HasValue ? filterStatus.GetDisplayName().ToLowerInvariant() : "All";
            if (filterStatus.HasValue)
            {
                switch (filterStatus.Value)
                {
                    case FilteringOptions.ClosingSoon:
                    case FilteringOptions.ClosingSoonWithNoApplications:
                    case FilteringOptions.AllApplications:
                    case FilteringOptions.NewApplications:
                        return $"{totalVacancies} {"vacancy".ToQuantity(totalVacancies, ShowQuantityAs.None)} {filterText}";
                    default:
                        return $"{totalVacancies} {filterText} {"vacancy".ToQuantity(totalVacancies, ShowQuantityAs.None)}";
                }
            }       
            return $"All {totalVacancies} vacancies";    
        }
    }
}