using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Employer.Web.Services
{
    public class EmployerAlertsViewModelFactory : IEmployerAlertsViewModelFactory
    {
        private readonly AlertViewModelService _alertViewModelService;
        private readonly IEmployerVacancyClient _employerVacancyClient;

        public EmployerAlertsViewModelFactory(AlertViewModelService alertViewModelService, IEmployerVacancyClient employerVacancyClient)
        {
            _alertViewModelService = alertViewModelService;
            _employerVacancyClient = employerVacancyClient;
        }
        public async Task<AlertsViewModel> Create(string employerAccountId, User user)
        {
            if (string.IsNullOrEmpty(employerAccountId))
                throw new ArgumentNullException(nameof(employerAccountId));

            var transferredVacancies = await _employerVacancyClient.GetDashboardAsync(employerAccountId, 1, FilteringOptions.Transferred, string.Empty);
            var employerRevokedTransferredVacanciesAlert = _alertViewModelService.GetTransferredVacanciesAlert(transferredVacancies.Vacancies, TransferReason.EmployerRevokedPermission, user.TransferredVacanciesEmployerRevokedPermissionAlertDismissedOn);
            var blockedProviderTransferredVacanciesAlert = _alertViewModelService.GetTransferredVacanciesAlert(transferredVacancies.Vacancies, TransferReason.BlockedByQa, user.TransferredVacanciesBlockedProviderAlertDismissedOn);
            
            var closedVacancies = await _employerVacancyClient.GetDashboardAsync(employerAccountId, 1, FilteringOptions.Closed, string.Empty);
            var blockedProviderAlert = _alertViewModelService.GetBlockedProviderVacanciesAlert(closedVacancies.Vacancies, user.ClosedVacanciesBlockedProviderAlertDismissedOn);
            var withdrawnByQaVacanciesAlert = _alertViewModelService.GetWithdrawnByQaVacanciesAlert(closedVacancies.Vacancies, user.ClosedVacanciesWithdrawnByQaAlertDismissedOn);

            return new AlertsViewModel(
                employerRevokedTransferredVacanciesAlert, 
                blockedProviderTransferredVacanciesAlert, 
                blockedProviderAlert,
                withdrawnByQaVacanciesAlert);
        }
    }
}
