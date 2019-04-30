using System;
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
    public class RecruitmentDashboardOrchestrator
    {
        //private const int VacanciesPerPage = 25;
        private readonly IProviderVacancyClient _client;
        private readonly ITimeProvider _timeProvider;

        public RecruitmentDashboardOrchestrator(IProviderVacancyClient client, ITimeProvider timeProvider)
        {
            _client = client;
            _timeProvider = timeProvider;
        }

        public async Task<RecruitmentDashboardViewModel> GetDashboardViewModelAsync(long ukprn, string filter, int page)
        {
            List<VacancySummary> vacancies = await GetVacanciesAsync(ukprn);

            //var filteringOption = SanitizeFilter(filter);

            //var filteredVacancies = GetFilteredVacancies(vacancies, filteringOption);                
            
            //var filteredVacanciesTotal = filteredVacancies.Count();

            //page = SanitizePage(page, filteredVacanciesTotal);

            //var skip = (page - 1) * VacanciesPerPage;

            //var vacanciesVm = filteredVacancies
            ////    .Skip(skip)
            ////    .Take(VacanciesPerPage)
            //    .Select(VacancySummaryMapper.ConvertToVacancySummaryViewModel)
            //    .ToList();

            var vm = new RecruitmentDashboardViewModel
            {
                Vacancies = vacancies,
                //Pager = pager,
                //Filter = filteringOption,
                //ResultsHeading = GetFilterHeading(filteredVacanciesTotal, filteringOption),
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
                        v.ClosingDate <= _timeProvider.Today.AddDays(5) && v.Status == VacancyStatus.Live);                    
                    break;
                case FilteringOptions.ClosingSoonWithNoApplications:
                    filteredVacancies = vacancies.Where(v =>
                        v.ClosingDate <= _timeProvider.Today.AddDays(5) && v.Status == VacancyStatus.Live && (v.NoOfSuccessfulApplications == 0 || v.NoOfUnsuccessfulApplications == 0 ||
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

        //private int SanitizePage(int page, int totalVacancies)
        //{
        //    return (page < 0 || page > (int)Math.Ceiling((double)totalVacancies / VacanciesPerPage)) ? 1 : page;
        //}

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