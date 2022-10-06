using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Employer.Web.ViewModels.Vacancies;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Shared.Web.Helpers;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class VacanciesOrchestrator
    {
        private const int VacanciesPerPage = 25;
        private readonly IEmployerVacancyClient _vacancyClient;
        private readonly IRecruitVacancyClient _client;
        private readonly IEmployerAlertsViewModelFactory _alertsViewModelFactory;

        public VacanciesOrchestrator(IEmployerVacancyClient vacancyClient, IRecruitVacancyClient client, IEmployerAlertsViewModelFactory alertsViewModelFactory)
        {
            _vacancyClient = vacancyClient;
            _client = client;
            _alertsViewModelFactory = alertsViewModelFactory;
        }

        public async Task<VacanciesViewModel> GetVacanciesViewModelAsync(string employerAccountId, string filter, int page, VacancyUser user, string searchTerm)
        {
            var filteringOption = SanitizeFilter(filter);
            var vacanciesTask = _vacancyClient.GetDashboardAsync(employerAccountId, page, filteringOption, searchTerm);
            var userDetailsTask = _client.GetUsersDetailsAsync(user.UserId);
            var employerVacancyCountTask = _vacancyClient.GetVacancyCount(employerAccountId, VacancyType.Apprenticeship, filteringOption, searchTerm);

            await Task.WhenAll(vacanciesTask, userDetailsTask, employerVacancyCountTask);

            var employerDashboard = vacanciesTask.Result;
            var userDetails = userDetailsTask.Result;
            var vacancyCount = employerVacancyCountTask.Result;
            var totalItems = Convert.ToInt32(vacancyCount);

            var vacancies = new List<VacancySummary>(employerDashboard?.Vacancies ?? Array.Empty<VacancySummary>());
            page = SanitizePage(page, totalItems);

            var vacanciesVm = vacancies
                .Select(VacancySummaryMapper.ConvertToVacancySummaryViewModel)
                .ToList();

            var pager = new PagerViewModel(
                totalItems,
                VacanciesPerPage,
                page,
                "Showing {0} to {1} of {2} adverts",
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
                ResultsHeading = VacancyFilterHeadingHelper.GetFilterHeading(Constants.VacancyTerm, vacancies.Count, filteringOption, searchTerm, UserType.Employer),
                Alerts = await _alertsViewModelFactory.Create(employerAccountId, userDetails)
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
    }
}