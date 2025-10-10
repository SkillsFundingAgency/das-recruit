namespace Esfa.Recruit.Employer.Web.ViewModels.Alerts
{
    public class AlertsViewModel
    {
        public EmployerTransferredVacanciesAlertViewModel EmployerRevokedTransferredVacanciesAlert { get; internal set; }
        public EmployerTransferredVacanciesAlertViewModel BlockedProviderTransferredVacanciesAlert { get; internal set; }
        public BlockedProviderAlertViewModel BlockedProviderAlert { get; internal set; }
        public WithdrawnVacanciesAlertViewModel WithdrawnByQaVacanciesAlert { get; internal set; }

        public bool ShowEmployerRevokedTransferredVacanciesAlert => EmployerRevokedTransferredVacanciesAlert is
        {
            TransferredVacanciesCount: > 0
        };
        public bool ShowBlockedProviderTransferredVacanciesAlert => BlockedProviderTransferredVacanciesAlert is
        {
            TransferredVacanciesCount: > 0
        };
        public bool ShowBlockedProviderAlert => BlockedProviderAlert is
        {
            HasMultipleBlockedProviders: true
        };
        public bool ShowWithdrawnByQaVacanciesAlert => WithdrawnByQaVacanciesAlert is
        {
            ClosedVacanciesCount: > 0
        };

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
