namespace Esfa.Recruit.Employer.Web.ViewModels.Dashboard
{
    public class AlertsViewModel
    {
        public TransferredVacanciesAlertViewModel EmployerRevokedTransferredVacanciesAlert { get; internal set; }
        public TransferredVacanciesAlertViewModel BlockedProviderTransferredVacanciesAlert { get; internal set; }
        public BlockedProviderAlertViewModel BlockedProviderAlert { get; internal set; }
        public WithdrawnVacanciesAlertViewModel WithdrawnVacanciesAlert { get; internal set; }

        public bool ShowEmployerRevokedTransferredVacanciesAlert => EmployerRevokedTransferredVacanciesAlert != null;
        public bool ShowBlockedProviderTransferredVacanciesAlert => BlockedProviderTransferredVacanciesAlert != null;
        public bool ShowBlockedProviderAlert => BlockedProviderAlert != null;
        public bool ShowWithdrawnVacanciesAlert => WithdrawnVacanciesAlert != null;
    }
}
