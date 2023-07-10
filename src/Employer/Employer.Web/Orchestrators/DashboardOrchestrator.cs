using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.Dashboard;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class DashboardOrchestrator
    {
        private readonly IEmployerVacancyClient _vacancyClient;
        private readonly IRecruitVacancyClient _client;
        private readonly IEmployerAlertsViewModelFactory _alertsViewModelFactory;
        private readonly IProviderRelationshipsService _providerRelationshipsService;

        public DashboardOrchestrator(
            IEmployerVacancyClient vacancyClient,
            IRecruitVacancyClient client,
            IEmployerAlertsViewModelFactory alertsViewModelFactory,
            IProviderRelationshipsService providerRelationshipsService)
        {
            _vacancyClient = vacancyClient;
            _client = client;
            _alertsViewModelFactory = alertsViewModelFactory;
            _providerRelationshipsService = providerRelationshipsService;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(string employerAccountId, VacancyUser user)
        {
            var dashboardTask = _vacancyClient.GetDashboardSummary(employerAccountId);
            var userDetailsTask = _client.GetUsersDetailsAsync(user.UserId);
            var providerTask = _providerRelationshipsService.CheckEmployerHasPermissions(employerAccountId, OperationType.RecruitmentRequiresReview);

            await Task.WhenAll(dashboardTask, userDetailsTask, providerTask);

            var dashboard = dashboardTask.Result;
            var userDetails = userDetailsTask.Result;

            if (userDetails == null)
            {
                var userOuter = await _client.GetEmployerIdentifiersAsync(user.UserId, user.Email);
                user.Name = $"{userOuter.FirstName} {userOuter.LastName}";
                await _client.UserSignedInAsync(user, UserType.Employer);
                userDetails = await _client.GetUsersDetailsAsync(user.UserId);
            }
            
            var providerPermissions = providerTask.Result;


            var vm = new DashboardViewModel
            {
                EmployerDashboardSummary = dashboard,
                EmployerAccountId = employerAccountId,
                Alerts = await _alertsViewModelFactory.Create(employerAccountId, userDetails),
                HasEmployerReviewPermission = providerPermissions
            };
            return vm;
        }
    }
}