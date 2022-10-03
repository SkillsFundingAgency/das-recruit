using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels.Alerts;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

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

            var employerRevokedTransfers = new EmployerTransferredVacanciesAlertViewModel
            {
                EmployerAccountId = employerAccountId,
                TransferredVacanciesCount = employerRevokedTransferredVacanciesAlert?.TransferredVacanciesCount ?? 0,
                TransferredVacanciesProviderNames = employerRevokedTransferredVacanciesAlert?.TransferredVacanciesProviderNames
            };
            var blockedProviderTransferred = new EmployerTransferredVacanciesAlertViewModel
            {
                EmployerAccountId = employerAccountId,
                TransferredVacanciesCount = blockedProviderTransferredVacanciesAlert?.TransferredVacanciesCount ?? 0,
                TransferredVacanciesProviderNames = blockedProviderTransferredVacanciesAlert?.TransferredVacanciesProviderNames
            };
            var blockedProvider = new BlockedProviderAlertViewModel
            {
                EmployerAccountId = employerAccountId,
                ClosedVacancies = blockedProviderAlert?.ClosedVacancies,
                BlockedProviderNames = blockedProviderAlert?.BlockedProviderNames
            };
            var withdrawn = new WithdrawnVacanciesAlertViewModel
            {
                EmployerAccountId = employerAccountId,
                ClosedVacancies = withdrawnByQaVacanciesAlert?.ClosedVacancies,
                Ukprn = withdrawnByQaVacanciesAlert?.Ukprn ?? default
            };
            
            return new AlertsViewModel(
                employerRevokedTransfers, 
                blockedProviderTransferred, 
                blockedProvider, 
                withdrawn);

        }
    }
}
