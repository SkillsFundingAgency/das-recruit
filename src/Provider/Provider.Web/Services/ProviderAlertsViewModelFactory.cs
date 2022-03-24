using System;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;

namespace Esfa.Recruit.Provider.Web.Services
{
    public class ProviderAlertsViewModelFactory : IProviderAlertsViewModelFactory
    {
        private readonly AlertViewModelService _alertViewModelService;

        public ProviderAlertsViewModelFactory(AlertViewModelService alertViewModelService)
        {
            _alertViewModelService = alertViewModelService;
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

            transferredVacanciesAlert.Ukprn = user.Ukprn.Value;
            withdrawnByQaVacanciesAlert.Ukprn = user.Ukprn.Value;
            
            return new AlertsViewModel(transferredVacanciesAlert, withdrawnByQaVacanciesAlert, user.Ukprn.Value);
        }
    }
}
