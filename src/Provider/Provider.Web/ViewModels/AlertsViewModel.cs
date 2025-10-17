using System.Linq;
using Esfa.Recruit.Shared.Web.ViewModels.Alerts;

namespace Esfa.Recruit.Provider.Web.ViewModels
{
    public class AlertsViewModel(
        ProviderTransferredVacanciesAlertViewModel transferredVacanciesAlert,
        WithdrawnVacanciesAlertViewModel withdrawnByQaVacanciesAlert,
        long? ukprn)
    {
        public ProviderTransferredVacanciesAlertViewModel TransferredVacanciesAlert { get; internal set; } = transferredVacanciesAlert;
        public WithdrawnVacanciesAlertViewModel WithdrawnByQaVacanciesAlert { get; internal set; } = withdrawnByQaVacanciesAlert;

        public bool ShowTransferredVacanciesAlert => TransferredVacanciesAlert is not null && TransferredVacanciesAlert.LegalEntityNames.Any();
        public bool ShowWithdrawnByQaVacanciesAlert => WithdrawnByQaVacanciesAlert is {ClosedVacanciesCount: > 0};
        public long? Ukprn { get; set; } = ukprn;
    }
}