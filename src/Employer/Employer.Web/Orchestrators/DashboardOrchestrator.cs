using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.Dashboard;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
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
            var dashboardTask = _vacancyClient.GetDashboardSummary(employerAccountId);
            var userDetailsTask = _client.GetUsersDetailsAsync(user.UserId);
            var providerTask = _providerRelationshipsService.GetLegalEntitiesForProviderAsync(employerAccountId, OperationType.RecruitmentRequiresReview);

            await Task.WhenAll(dashboardTask, userDetailsTask, providerTask);

            var dashboard = dashboardTask.Result;
            var userDetails = userDetailsTask.Result;
            var providerPermissions = providerTask.Result;


            var vm = new DashboardViewModel
            {
                EmployerDashboardSummary = dashboard,
                EmployerAccountId = employerAccountId,
                Alerts = await _alertsViewModelFactory.Create(employerAccountId, userDetails),
                HasEmployerReviewPermission = providerPermissions.Any()
            };
            return vm;
        }
    }
}