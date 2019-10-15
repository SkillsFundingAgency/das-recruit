using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Employer.Web.Services
{
    public class EmployerAlertsViewModelFactory : IEmployerAlertsViewModelFactory
    {
        private readonly AlertViewModelService _alertViewModelService;

        public EmployerAlertsViewModelFactory(AlertViewModelService alertViewModelService)
        {
            _alertViewModelService = alertViewModelService;
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
