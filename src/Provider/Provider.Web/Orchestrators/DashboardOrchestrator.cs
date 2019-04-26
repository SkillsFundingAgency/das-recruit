﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
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
    public class DashboardOrchestrator
    {
        private const int VacanciesPerPage = 25;
        private readonly IProviderVacancyClient _client;
        private readonly ITimeProvider _timeProvider;

        public DashboardOrchestrator(IProviderVacancyClient client, ITimeProvider timeProvider)
        {
            _client = client;
            _timeProvider = timeProvider;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(long ukprn, string filter, int page)
        {
            var vacancies = await GetVacanciesAsync(ukprn);

<<<<<<< HEAD
            var filteringOption = SanitizeFilter(filter);

            var filteredVacancies = GetFilteredVacancies(vacancies, filteringOption);                
=======
            var filteringOptions = SanitizeFilter(filter);

            var filteredVacancies = GetFilteredVacancies(vacancies, filteringOptions);                
>>>>>>> Implementing PR comments.
            
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
<<<<<<< HEAD
                    {"filter", filteringOption.ToString()}
=======
                    {"filter", filteringOptions.ToString()}
>>>>>>> Implementing PR comments.
                });
            
            var vm = new DashboardViewModel 
            {
                Vacancies = vacanciesVm,
                Pager = pager,
<<<<<<< HEAD
                Filter = filteringOption,
                ResultsHeading = GetFilterHeading(filteredVacanciesTotal, filteringOption),
=======
                Filter = filteringOptions,
                ResultsHeading = GetFilterHeading(filteredVacanciesTotal, filteringOptions),
>>>>>>> Implementing PR comments.
                HasVacancies = vacancies.Any()
            };

            return vm;
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
                    filteredVacancies = vacancies.Where(v =>
                        v.NoOfSuccessfulApplications > 0 || v.NoOfUnsuccessfulApplications > 0 ||
                        v.NoOfNewApplications > 0);
                    break;
                case FilteringOptions.ClosingSoon:
                    filteredVacancies = vacancies.Where(v =>
<<<<<<< HEAD
                        v.ClosingDate <= _timeProvider.Today.AddDays(5) && v.Status == VacancyStatus.Live);                    
                    break;
                case FilteringOptions.ClosingSoonWithNoApplications:
                    filteredVacancies = vacancies.Where(v =>
                        v.ClosingDate <= _timeProvider.Today.AddDays(5) && v.Status == VacancyStatus.Live && (v.NoOfSuccessfulApplications == 0 || v.NoOfUnsuccessfulApplications == 0 ||
=======
                        v.ClosingDate <= _timeProvider.Now.AddDays(5) && v.Status == VacancyStatus.Live);                    
                    break;
                case FilteringOptions.ClosingSoonWithNoApplications:
                    filteredVacancies = vacancies.Where(v =>
                        v.ClosingDate <= _timeProvider.Now.AddDays(5) && v.Status == VacancyStatus.Live && (v.NoOfSuccessfulApplications == 0 || v.NoOfUnsuccessfulApplications == 0 ||
>>>>>>> Implementing PR comments.
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
            return (page < 0 || page > (int)Math.Ceiling((double)totalVacancies / VacanciesPerPage)) ? 1 : page;
        }

        private FilteringOptions SanitizeFilter(string filter)
        {
            if (Enum.TryParse(typeof(FilteringOptions), filter, out var status))
                return (FilteringOptions)status;
            return FilteringOptions.All;
        }

<<<<<<< HEAD
        private string GetFilterHeading(int totalVacancies, FilteringOptions filteringOption)
        {
            var filterText = filteringOption.GetDisplayName().ToLowerInvariant();
            switch (filteringOption)
=======
        private string GetFilterHeading(int totalVacancies, FilteringOptions options)
        {
            var filterText = options.GetDisplayName().ToLowerInvariant();
            switch (options)
>>>>>>> Implementing PR comments.
            {
                case FilteringOptions.ClosingSoon:
                case FilteringOptions.ClosingSoonWithNoApplications:
                case FilteringOptions.AllApplications:
                case FilteringOptions.NewApplications:
                    return $"{totalVacancies} {"vacancy".ToQuantity(totalVacancies, ShowQuantityAs.None)} {filterText}";
                case FilteringOptions.All:
                    return $"All {totalVacancies} vacancies";
                default:
                    return $"{totalVacancies} {filterText} {"vacancy".ToQuantity(totalVacancies, ShowQuantityAs.None)}";
            }                                  
        }        
    }
}