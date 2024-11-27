using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;

namespace Esfa.Recruit.Provider.Web.Services
{
    public class ProviderAlertsViewModelFactory : IProviderAlertsViewModelFactory
    {
        private readonly AlertViewModelService _alertViewModelService;
        private readonly IProviderVacancyClient _providerVacancyClient;
        private readonly ServiceParameters _serviceParameters;

        public ProviderAlertsViewModelFactory(AlertViewModelService alertViewModelService, IProviderVacancyClient providerVacancyClient, ServiceParameters serviceParameters)
        {
            _alertViewModelService = alertViewModelService;
            _providerVacancyClient = providerVacancyClient;
            _serviceParameters = serviceParameters;
        }
        public async Task<AlertsViewModel> Create(User user)
        {
            if (user?.Ukprn == null)
                throw new ArgumentNullException(nameof(user));

            var vacancies = await _providerVacancyClient.GetDashboardAsync(user.Ukprn.Value, 1, FilteringOptions.Closed, null);
            
            var transferredVacanciesAlert = _alertViewModelService.GetProviderTransferredVacanciesAlert(
                vacancies.TransferredVacancies,
                user.TransferredVacanciesEmployerRevokedPermissionAlertDismissedOn);
            var withdrawnByQaVacanciesAlert = _alertViewModelService.GetWithdrawnByQaVacanciesAlert(
                vacancies.Vacancies,
                user.ClosedVacanciesWithdrawnByQaAlertDismissedOn);

            if (user.Ukprn.HasValue)
            {
                if (transferredVacanciesAlert != null)
                {
                    transferredVacanciesAlert.Ukprn = user.Ukprn.Value;    
                    
                }
                if (withdrawnByQaVacanciesAlert != null)
                {
                    withdrawnByQaVacanciesAlert.Ukprn = user.Ukprn.Value;    
                }
            }
            
            return new AlertsViewModel(transferredVacanciesAlert, withdrawnByQaVacanciesAlert, user.Ukprn);
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

            if (user.Ukprn.HasValue)
            {
                if (transferredVacanciesAlert != null)
                {
                    transferredVacanciesAlert.Ukprn = user.Ukprn.Value;    
                    
                }
                if (withdrawnByQaVacanciesAlert != null)
                {
                    withdrawnByQaVacanciesAlert.Ukprn = user.Ukprn.Value;    
                }
            }
            
            return new AlertsViewModel(transferredVacanciesAlert, withdrawnByQaVacanciesAlert, user.Ukprn);
        }
    }
}
