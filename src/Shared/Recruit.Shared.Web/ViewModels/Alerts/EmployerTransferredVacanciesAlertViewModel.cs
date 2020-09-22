using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.Extensions;
using Humanizer;

namespace Esfa.Recruit.Shared.Web.ViewModels.Alerts
{
    public class EmployerTransferredVacanciesAlertViewModel
    {
        public int TransferredVacanciesCount { get; internal set; }
        public List<string> TransferredVacanciesProviderNames { get; internal set; }

        public string CountCaption => $"{"advert".ToQuantity(TransferredVacanciesCount)} {(TransferredVacanciesCount == 1 ? "One advert has" : "have")} been transferred";
        public string ProviderNamesCaption => TransferredVacanciesProviderNames.Humanize().RemoveOxfordComma();
        public bool HasTransfersFromMultipleProviders => TransferredVacanciesProviderNames.Count() > 1;
    }
}
