using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class DashboardOrchestrator
    {
        private const int ClosingSoonDays = 5;
        private readonly IProviderVacancyClient _vacancyClient;
        private readonly ITimeProvider _timeProvider;
        private readonly IRecruitVacancyClient _client;
        private readonly AlertViewModelService _alertViewModelService;
        private readonly IProviderAlertsViewModelFactory _providerAlertsViewModelFactory;

        public DashboardOrchestrator(
            IProviderVacancyClient vacancyClient,
            ITimeProvider timeProvider,
            IRecruitVacancyClient client,
            AlertViewModelService alertViewModelService,
            IProviderAlertsViewModelFactory providerAlertsViewModelFactory)
        {
            _vacancyClient = vacancyClient;
            _timeProvider = timeProvider;
            _client = client;
            _alertViewModelService = alertViewModelService;
            _providerAlertsViewModelFactory = providerAlertsViewModelFactory;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(VacancyUser user)
        {
            var dashboardTask = _vacancyClient.GetDashboardAsync(user.Ukprn.Value, createIfNonExistent: true);
            var userDetailsTask = _client.GetUsersDetailsAsync(user.UserId);

            await Task.WhenAll(dashboardTask, userDetailsTask);

            var dashboard = dashboardTask.Result;
            var userDetails = userDetailsTask.Result;

            var vacancies = dashboard.Vacancies?.ToList() ?? new List<VacancySummary>();
            var transferredVacancies = dashboard.TransferredVacancies?.ToList() ?? new List<ProviderDashboardTransferredVacancy>();

            var vm = new DashboardViewModel
            {
                Vacancies = vacancies,
                HasAnyVacancies = vacancies.Any(),
                NoOfVacanciesClosingSoonWithNoApplications = vacancies.Count(v =>
                    v.ClosingDate <= _timeProvider.Today.AddDays(ClosingSoonDays) &&
                    v.Status == VacancyStatus.Live &&
                    v.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship &&
                    v.NoOfApplications == 0),
                NoOfVacanciesClosingSoon = vacancies.Count(v =>
                    v.ClosingDate <= _timeProvider.Today.AddDays(ClosingSoonDays) &&
                    v.Status == VacancyStatus.Live),
                Alerts = _providerAlertsViewModelFactory.Create(dashboard, userDetails)
            };
            return vm;
        }

        public Task DismissAlert(VacancyUser user, AlertType alertType)
        {
            return _client.UpdateUserAlertAsync(user.UserId, alertType, _timeProvider.Now);
        }


    }
}