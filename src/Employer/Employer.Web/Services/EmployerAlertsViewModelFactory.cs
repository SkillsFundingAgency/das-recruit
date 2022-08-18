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

            // transferred vacancies
            var employerRevokedTransferredVacanciesAlert = _alertViewModelService.GetTransferredVacanciesAlert(new List<VacancySummary>(), TransferReason.EmployerRevokedPermission, user.TransferredVacanciesEmployerRevokedPermissionAlertDismissedOn);
            var blockedProviderTransferredVacanciesAlert = _alertViewModelService.GetTransferredVacanciesAlert(new List<VacancySummary>(), TransferReason.BlockedByQa, user.TransferredVacanciesBlockedProviderAlertDismissedOn);
            
            //TODO pass list of closed vacancies
            var blockedProviderAlert = _alertViewModelService.GetBlockedProviderVacanciesAlert(new List<VacancySummary>(), user.ClosedVacanciesBlockedProviderAlertDismissedOn);
            var withdrawnByQaVacanciesAlert = _alertViewModelService.GetWithdrawnByQaVacanciesAlert(new List<VacancySummary>(), user.ClosedVacanciesWithdrawnByQaAlertDismissedOn);

            return new AlertsViewModel(
                employerRevokedTransferredVacanciesAlert, 
                blockedProviderTransferredVacanciesAlert, 
                blockedProviderAlert,
                withdrawnByQaVacanciesAlert);
        }

        public AlertsViewModel Create(IEnumerable<VacancySummary> vacancies, User user)
        {
            var employerRevokedTransferredVacanciesAlert = _alertViewModelService.GetTransferredVacanciesAlert(vacancies, TransferReason.EmployerRevokedPermission, user.TransferredVacanciesEmployerRevokedPermissionAlertDismissedOn);
            var blockedProviderTransferredVacanciesAlert = _alertViewModelService.GetTransferredVacanciesAlert(vacancies, TransferReason.BlockedByQa, user.TransferredVacanciesBlockedProviderAlertDismissedOn);
            var blockedProviderAlert = _alertViewModelService.GetBlockedProviderVacanciesAlert(vacancies, user.ClosedVacanciesBlockedProviderAlertDismissedOn);
            var withdrawnByQaVacanciesAlert = _alertViewModelService.GetWithdrawnByQaVacanciesAlert(vacancies, user.ClosedVacanciesWithdrawnByQaAlertDismissedOn);

            return new AlertsViewModel(
                employerRevokedTransferredVacanciesAlert, 
                blockedProviderTransferredVacanciesAlert, 
                blockedProviderAlert,
                withdrawnByQaVacanciesAlert);
        }
    }
}
