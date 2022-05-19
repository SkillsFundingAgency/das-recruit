using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Provider.Web.ViewModels.Dashboard;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class DashboardOrchestrator
    {
        private const int ClosingSoonDays = 5;
        private readonly IProviderVacancyClient _vacancyClient;
        private readonly ITimeProvider _timeProvider;
        private readonly IRecruitVacancyClient _client;
        private readonly IProviderAlertsViewModelFactory _providerAlertsViewModelFactory;
        private readonly IProviderRelationshipsService _providerRelationshipsService;
        private readonly ServiceParameters _serviceParameters;

        public DashboardOrchestrator(
            IProviderVacancyClient vacancyClient,
            ITimeProvider timeProvider,
            IRecruitVacancyClient client,
            IProviderAlertsViewModelFactory providerAlertsViewModelFactory,
            IProviderRelationshipsService providerRelationshipsService,
            ServiceParameters serviceParameters)
        {
            _vacancyClient = vacancyClient;
            _timeProvider = timeProvider;
            _client = client;
            _providerAlertsViewModelFactory = providerAlertsViewModelFactory;
            _providerRelationshipsService = providerRelationshipsService;
            _serviceParameters = serviceParameters;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(VacancyUser user)
        {
            var serviceParametersVacancyType = _serviceParameters.VacancyType.GetValueOrDefault();
            var dashboardTask = _vacancyClient.GetDashboardAsync(user.Ukprn.Value, serviceParametersVacancyType, true);
            var userDetailsTask = _client.GetUsersDetailsAsync(user.UserId);
            var providerTask = _providerRelationshipsService.GetLegalEntitiesForProviderAsync(user.Ukprn.Value, serviceParametersVacancyType == VacancyType.Apprenticeship ? OperationType.RecruitmentRequiresReview : OperationType.Recruitment);

            await Task.WhenAll(dashboardTask, userDetailsTask, providerTask);

            var dashboard = dashboardTask.Result;
            var userDetails = userDetailsTask.Result;
            var providerPermissions = providerTask.Result;

            var vacancies = dashboard.Vacancies?.ToList() ?? new List<VacancySummary>();

            var vm = new DashboardViewModel
            {
                Vacancies = vacancies,
                NoOfVacanciesClosingSoonWithNoApplications = vacancies.Count(v =>
                    v.ClosingDate <= _timeProvider.Today.AddDays(ClosingSoonDays) &&
                    v.Status == VacancyStatus.Live &&
                    v.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship &&
                    v.NoOfApplications == 0),
                NoOfVacanciesClosingSoon = vacancies.Count(v =>
                    v.ClosingDate <= _timeProvider.Today.AddDays(ClosingSoonDays) &&
                    v.Status == VacancyStatus.Live),
                Alerts = _providerAlertsViewModelFactory.Create(dashboard, userDetails),
                HasEmployerReviewPermission = providerPermissions.Any(),
                Ukprn = user.Ukprn.Value
            };
            return vm;
        }
    }
}