using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Humanizer;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class VacanciesOrchestrator
    {
        private const int VacanciesPerPage = 25;
        private readonly IProviderVacancyClient _client;
        private readonly ITimeProvider _timeProvider;
        private const int ClosingSoonDays = 5;

        public VacanciesOrchestrator(IProviderVacancyClient client, ITimeProvider timeProvider)
        {
            _client = client;
            _timeProvider = timeProvider;
        }

        public async Task<VacanciesViewModel> GetVacanciesViewModelAsync(
            VacancyUser user, string filter, int page, string searchTerm)
        {
            var vacancies = await GetVacanciesAsync(user.Ukprn.Value);

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
                RouteNames.Vacancies_Get,
                new Dictionary<string, string>
                {
                    {"filter", filteringOption.ToString()},
                    {"searchTerm", searchTerm}
                });
            
            var vm = new VacanciesViewModel 
            {
                Vacancies = vacanciesVm,
                Pager = pager,
                Filter = filteringOption,
                SearchTerm = searchTerm,
                ResultsHeading = GetFilterHeading(filteredVacanciesTotal, filteringOption, searchTerm),
                HasAnyVacancies = vacancies.Any()
            };

            return vm;
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
            }
            return filteredVacancies
                .Where(v => string.IsNullOrWhiteSpace(searchTerm)  
                    || (v.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) 
                        || (string.IsNullOrWhiteSpace(v.LegalEntityName) == false && v.LegalEntityName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        || (v.VacancyReference.HasValue && $"VAC{v.VacancyReference}".Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(v => v.CreatedDate)

                .ToList(); 
        }

        private async Task<List<VacancySummary>> GetVacanciesAsync(long ukprn)
        {
            var dashboard = await _client.GetDashboardAsync(ukprn, createIfNonExistent: true);

            if (dashboard == null)
            {
                await _client.GenerateDashboard(ukprn);
                dashboard = await _client.GetDashboardAsync(ukprn);
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
    }
}