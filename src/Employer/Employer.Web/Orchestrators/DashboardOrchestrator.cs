using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.Dashboard;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class DashboardOrchestrator
    {
        private const int ClosingSoonDays = 5;
        private readonly IEmployerVacancyClient _vacancyClient;
        private readonly ITimeProvider _timeProvider;
        private readonly IRecruitVacancyClient _client;
        private readonly IEmployerAlertsViewModelFactory _alertsViewModelFactory;
        private readonly IProviderRelationshipsService _providerRelationshipsService;

        public DashboardOrchestrator(
            IEmployerVacancyClient vacancyClient,
            ITimeProvider timeProvider,
            IRecruitVacancyClient client,
            IEmployerAlertsViewModelFactory alertsViewModelFactory,
            IProviderRelationshipsService providerRelationshipsService)
        {
            _vacancyClient = vacancyClient;
            _timeProvider = timeProvider;
            _client = client;
            _alertsViewModelFactory = alertsViewModelFactory;
            _providerRelationshipsService = providerRelationshipsService;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(string employerAccountId, VacancyUser user)
        {
            var dashboardTask = _vacancyClient.GetDashboardAsync(employerAccountId, createIfNonExistent: true);
            var userDetailsTask = _client.GetUsersDetailsAsync(user.UserId);
            var providerTask = _providerRelationshipsService.GetLegalEntitiesForProviderAsync(employerAccountId, "RecruitmentRequiresReview"); //TODO : check

            await Task.WhenAll(dashboardTask, userDetailsTask, providerTask);

            var dashboard = dashboardTask.Result;
            var userDetails = userDetailsTask.Result;
            var providerPermissions = providerTask.Result;

            var vacancies = dashboard.Vacancies?.ToList() ?? new List<VacancySummary>();

            var vm = new DashboardViewModel
            {
                EmployerAccountId = employerAccountId,
                Vacancies = vacancies,
                NoOfVacanciesClosingSoonWithNoApplications = vacancies.Count(v =>
                    v.ClosingDate <= _timeProvider.Today.AddDays(ClosingSoonDays) &&
                    v.Status == VacancyStatus.Live &&
                    v.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship &&
                    v.NoOfApplications == 0),
                NoOfVacanciesClosingSoon = vacancies.Count(v =>
                    v.ClosingDate <= _timeProvider.Today.AddDays(ClosingSoonDays) &&
                    v.Status == VacancyStatus.Live),
                Alerts = _alertsViewModelFactory.Create(vacancies, userDetails),
                HasEmployerReviewPermission = providerPermissions.Any() //TODO : check
            };
            return vm;
        }
    }
}