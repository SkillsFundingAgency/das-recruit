

namespace Esfa.Recruit.Employer.Web.ViewModels.Alerts
{
    public class AlertsViewModel
    {
        public EmployerTransferredVacanciesAlertViewModel EmployerRevokedTransferredVacanciesAlert { get; internal set; }
        public EmployerTransferredVacanciesAlertViewModel BlockedProviderTransferredVacanciesAlert { get; internal set; }
        public BlockedProviderAlertViewModel BlockedProviderAlert { get; internal set; }
        public WithdrawnVacanciesAlertViewModel WithdrawnByQaVacanciesAlert { get; internal set; }

        public bool ShowEmployerRevokedTransferredVacanciesAlert => EmployerRevokedTransferredVacanciesAlert != null;
        public bool ShowBlockedProviderTransferredVacanciesAlert => BlockedProviderTransferredVacanciesAlert != null;
        public bool ShowBlockedProviderAlert => BlockedProviderAlert != null;
        public bool ShowWithdrawnByQaVacanciesAlert => WithdrawnByQaVacanciesAlert != null;

        public AlertsViewModel(EmployerTransferredVacanciesAlertViewModel employerRevokedTransferredVacanciesAlert,
            EmployerTransferredVacanciesAlertViewModel blockedProviderTransferredVacanciesAlert,
            BlockedProviderAlertViewModel blockedProviderAlert,
            WithdrawnVacanciesAlertViewModel withdrawnByQaVacanciesAlert)
        {
            EmployerRevokedTransferredVacanciesAlert = employerRevokedTransferredVacanciesAlert;
            BlockedProviderTransferredVacanciesAlert = blockedProviderTransferredVacanciesAlert;
            BlockedProviderAlert = blockedProviderAlert;
            WithdrawnByQaVacanciesAlert = withdrawnByQaVacanciesAlert;
        }
    }
}
