using Esfa.Recruit.Shared.Web.ViewModels.Alerts;

namespace Esfa.Recruit.Provider.Web.ViewModels
{
    public class AlertsViewModel
    {
        public ProviderTransferredVacanciesAlertViewModel TransferredVacanciesAlert { get; internal set; }
        public WithdrawnVacanciesAlertViewModel WithdrawnByQaVacanciesAlert { get; internal set; }

        public bool ShowTransferredVacanciesAlert => TransferredVacanciesAlert != null;
        public bool ShowWithdrawnByQaVacanciesAlert => WithdrawnByQaVacanciesAlert != null;

        public AlertsViewModel(ProviderTransferredVacanciesAlertViewModel transferredVacanciesAlert, WithdrawnVacanciesAlertViewModel withdrawnByQaVacanciesAlert)
        {
            TransferredVacanciesAlert = transferredVacanciesAlert;
            WithdrawnByQaVacanciesAlert = withdrawnByQaVacanciesAlert;
        }
    }
}
