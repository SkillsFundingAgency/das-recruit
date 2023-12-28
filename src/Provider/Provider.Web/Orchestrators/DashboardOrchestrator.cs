using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Provider.Web.ViewModels.Dashboard;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class DashboardOrchestrator
    {
        private readonly IProviderVacancyClient _vacancyClient;
        private readonly IRecruitVacancyClient _client;
        private readonly IProviderAlertsViewModelFactory _providerAlertsViewModelFactory;
        private readonly IProviderRelationshipsService _providerRelationshipsService;
        private readonly ServiceParameters _serviceParameters;

        public DashboardOrchestrator(
            IProviderVacancyClient vacancyClient,
            IRecruitVacancyClient client,
            IProviderAlertsViewModelFactory providerAlertsViewModelFactory,
            IProviderRelationshipsService providerRelationshipsService,
            ServiceParameters serviceParameters)
        {
            _vacancyClient = vacancyClient;
            _client = client;
            _providerAlertsViewModelFactory = providerAlertsViewModelFactory;
            _providerRelationshipsService = providerRelationshipsService;
            _serviceParameters = serviceParameters;
        }

        public virtual async Task<DashboardViewModel> GetDashboardViewModelAsync(VacancyUser user)
        {
            await _client.UserSignedInAsync(user, UserType.Provider);
            var serviceParametersVacancyType = _serviceParameters.VacancyType.GetValueOrDefault();
            var dashboardTask = _vacancyClient.GetDashboardSummary(user.Ukprn.Value, serviceParametersVacancyType);
            var userDetailsTask = _client.GetUsersDetailsAsync(user.UserId);
            var providerTask = serviceParametersVacancyType == VacancyType.Apprenticeship 
                ? _providerRelationshipsService.CheckProviderHasPermissions(user.Ukprn.Value, OperationType.RecruitmentRequiresReview)
                : Task.FromResult(false);

            await Task.WhenAll(dashboardTask, userDetailsTask, providerTask);

            var dashboard = dashboardTask.Result;
            var userDetails = userDetailsTask.Result;
            var providerPermissions = providerTask.Result;

            var alerts = await _providerAlertsViewModelFactory.Create(userDetails);
            
            var vm = new DashboardViewModel
            {
                ProviderDashboardSummary = dashboard,
                Alerts = alerts,
                HasEmployerReviewPermission = providerPermissions,
                Ukprn = user.Ukprn.Value
            };
            return vm;
        }
    }
}