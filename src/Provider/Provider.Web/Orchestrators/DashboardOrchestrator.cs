using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Provider.Web.ViewModels.Dashboard;
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

        public DashboardOrchestrator(IProviderVacancyClient vacancyClient, ITimeProvider timeProvider, IRecruitVacancyClient client)
        {
            _vacancyClient = vacancyClient;
            _timeProvider = timeProvider;
            _client = client;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(VacancyUser user)
        {
            var dashboardTask = GetDashboardAsync(user.Ukprn.Value);
            var userDetailsTask = _client.GetUsersDetailsAsync(user.UserId);

            await Task.WhenAll(dashboardTask, userDetailsTask);

            var dashboard = dashboardTask.Result;
            var userDetails = userDetailsTask.Result;

            var vacancies = dashboard.Vacancies?.ToList() ?? new List<VacancySummary>();

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
                TransferredVacanciesAlert = GetTransferredVacanciesAlertViewModel(dashboard.TransferredVacancies, userDetails.TransferredVacanciesAlertDismissedOn)
            };
            return vm;
        }

        public Task DismissAlert(VacancyUser user, AlertType alertType)
        {
            return _client.UpdateUserAlertAsync(user.UserId, alertType, _timeProvider.Now);
        }

        private async Task<ProviderDashboard> GetDashboardAsync(long ukprn)
        {
            var dashboard = await _vacancyClient.GetDashboardAsync(ukprn);

            if (dashboard == null)
            {
                await _vacancyClient.GenerateDashboard(ukprn);
                dashboard = await _vacancyClient.GetDashboardAsync(ukprn);
            }

            return dashboard;
        }

        private TransferredVacanciesAlertViewModel GetTransferredVacanciesAlertViewModel(IEnumerable<ProviderDashboardTransferredVacancy> transferredVacancies, DateTime? userLastDismissedDate)
        {
            if (transferredVacancies == null)
                return null;

            if (userLastDismissedDate.HasValue == false)
                userLastDismissedDate = DateTime.MinValue;

            var usersTransferredVacancies = transferredVacancies
                .Where(t => t.TransferredDate > userLastDismissedDate)
                .Select(t => t.LegalEntityName);
            
            if (usersTransferredVacancies.Any() == false)
                return null;

            return new TransferredVacanciesAlertViewModel
            {
                LegalEntityNames = usersTransferredVacancies.GroupBy(l => l).Select(l => l.Key)
            };
        }
    }
}