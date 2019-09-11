using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class VacanciesOrchestrator
    {
        private const int ClosingSoonDays = 5;
        private const int VacanciesPerPage = 25;
        private readonly IProviderVacancyClient _providerVacancyClient;
        private readonly IRecruitVacancyClient _recruitVacancyClient;
        private readonly ITimeProvider _timeProvider;
        private readonly IProviderAlertsViewModelFactory _providerAlertsViewModelFactory;

        public VacanciesOrchestrator(
            IProviderVacancyClient providerVacancyClient,
            IRecruitVacancyClient recruitVacancyClient,
            ITimeProvider timeProvider,
            IProviderAlertsViewModelFactory providerAlertsViewModelFactory)
        {
            _providerVacancyClient = providerVacancyClient;
            _recruitVacancyClient = recruitVacancyClient;
            _timeProvider = timeProvider;
            _providerAlertsViewModelFactory = providerAlertsViewModelFactory;
        }

        public async Task<VacanciesViewModel> GetVacanciesViewModelAsync(
            VacancyUser user, string filter, int page, string searchTerm)
        {
            var getDashboardTask = _providerVacancyClient.GetDashboardAsync(user.Ukprn.Value, createIfNonExistent: true);
            var getUserDetailsTask = _recruitVacancyClient.GetUsersDetailsAsync(user.UserId);

            await Task.WhenAll(getDashboardTask, getUserDetailsTask);

            var dashboard = getDashboardTask.Result;
            var userDetails = getUserDetailsTask.Result;

            var alerts = _providerAlertsViewModelFactory.Create(dashboard, userDetails);

            var vacancies = new List<VacancySummary>(dashboard?.Vacancies ?? Array.Empty<VacancySummary>());

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
                ResultsHeading = VacancyFilterHeadingHelper.GetFilterHeading(filteredVacanciesTotal, filteringOption, searchTerm),
                HasAnyVacancies = vacancies.Any(),
                Alerts = alerts
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
    }
}