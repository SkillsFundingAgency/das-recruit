using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels.Alerts;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Services
{
    public class EmployerAlertsViewModelFactory(
        AlertViewModelService alertViewModelService,
        IEmployerVacancyClient employerVacancyClient)
        : IEmployerAlertsViewModelFactory
    {
        public async Task<AlertsViewModel> Create(string employerAccountId, User user)
        {
            if (string.IsNullOrEmpty(employerAccountId))
                throw new ArgumentNullException(nameof(employerAccountId));

            var transferredVacancies = await employerVacancyClient.GetDashboardAsync(employerAccountId, "", 1, 25, "", "", FilteringOptions.Transferred, null); ;
            var employerRevokedTransferredVacanciesAlert = alertViewModelService.GetTransferredVacanciesAlert(transferredVacancies.Vacancies, TransferReason.EmployerRevokedPermission, user.TransferredVacanciesEmployerRevokedPermissionAlertDismissedOn);
            var blockedProviderTransferredVacanciesAlert = alertViewModelService.GetTransferredVacanciesAlert(transferredVacancies.Vacancies, TransferReason.BlockedByQa, user.TransferredVacanciesBlockedProviderAlertDismissedOn);
            
            var closedVacancies = await employerVacancyClient.GetDashboardAsync(employerAccountId, "", 1, 25, "", "", FilteringOptions.Closed, null);
            var blockedProviderAlert = alertViewModelService.GetBlockedProviderVacanciesAlert(closedVacancies.Vacancies, user.ClosedVacanciesBlockedProviderAlertDismissedOn);
            var withdrawnByQaVacanciesAlert = alertViewModelService.GetWithdrawnByQaVacanciesAlert(closedVacancies.Vacancies, user.ClosedVacanciesWithdrawnByQaAlertDismissedOn);

            var employerRevokedTransfers = employerRevokedTransferredVacanciesAlert == null ? null: new EmployerTransferredVacanciesAlertViewModel
            {
                EmployerAccountId = employerAccountId,
                TransferredVacanciesCount = employerRevokedTransferredVacanciesAlert.TransferredVacanciesCount,
                TransferredVacanciesProviderNames = employerRevokedTransferredVacanciesAlert.TransferredVacanciesProviderNames
            };
            var blockedProviderTransferred = blockedProviderTransferredVacanciesAlert == null ? null: new EmployerTransferredVacanciesAlertViewModel
            {
                EmployerAccountId = employerAccountId,
                TransferredVacanciesCount = (int) blockedProviderTransferredVacanciesAlert.TransferredVacanciesCount,
                TransferredVacanciesProviderNames = blockedProviderTransferredVacanciesAlert.TransferredVacanciesProviderNames
            };
            var blockedProvider = blockedProviderAlert == null ? null : new BlockedProviderAlertViewModel
            {
                EmployerAccountId = employerAccountId,
                ClosedVacancies = blockedProviderAlert.ClosedVacancies,
                BlockedProviderNames = blockedProviderAlert.BlockedProviderNames
            };
            var withdrawn = withdrawnByQaVacanciesAlert == null ? null : new WithdrawnVacanciesAlertViewModel
            {
                EmployerAccountId = employerAccountId,
                ClosedVacancies = withdrawnByQaVacanciesAlert.ClosedVacancies,
                Ukprn = (long) withdrawnByQaVacanciesAlert.Ukprn
            };
            
            return new AlertsViewModel(
                employerRevokedTransfers, 
                blockedProviderTransferred, 
                blockedProvider, 
                withdrawn);

        }
    }
}
