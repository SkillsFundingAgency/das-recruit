using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;

namespace Esfa.Recruit.Provider.Web.Services
{
    public class ProviderAlertsViewModelFactory : IProviderAlertsViewModelFactory
    {
        private readonly IProviderVacancyClient _providerVacancyClient;
        private readonly AlertViewModelService _alertViewModelService;

        public ProviderAlertsViewModelFactory(IProviderVacancyClient providerVacancyClient, AlertViewModelService alertViewModelService)
        {
            _providerVacancyClient = providerVacancyClient;
            _alertViewModelService = alertViewModelService;
        }

        public async Task<AlertsViewModel> CreateAsync(long ukprn, User user)
        {
            var dashboard = await _providerVacancyClient.GetDashboardAsync(ukprn, createIfNonExistent: true);
            return Create(dashboard, user);
        }

        public AlertsViewModel Create(ProviderDashboard providerDashboard, User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var transferredVacanciesAlert = _alertViewModelService.GetProviderTransferredVacanciesAlert(
                providerDashboard?.TransferredVacancies ?? Array.Empty<ProviderDashboardTransferredVacancy>(),
                user.TransferredVacanciesEmployerRevokedPermissionAlertDismissedOn);
            var withdrawnByQaVacanciesAlert = _alertViewModelService.GetWithdrawnByQaVacanciesAlert(
                providerDashboard?.Vacancies ?? Array.Empty<VacancySummary>(),
                user.ClosedVacanciesWithdrawnByQaAlertDismissedOn);

            return new AlertsViewModel(transferredVacanciesAlert, withdrawnByQaVacanciesAlert);
        }
    }
}
