using Esfa.Recruit.Shared.Web.ViewModels.Alerts;

namespace Esfa.Recruit.Employer.Web.ViewModels
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
    }
}
