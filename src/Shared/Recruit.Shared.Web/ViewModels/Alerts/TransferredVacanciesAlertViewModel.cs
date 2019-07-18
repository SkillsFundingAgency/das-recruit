using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.Extensions;
using Humanizer;

namespace Esfa.Recruit.Shared.Web.ViewModels.Alerts
{
    public class TransferredVacanciesAlertViewModel
    {
        public int TransferredVacanciesCount { get; internal set; }
        public IEnumerable<string> TransferredVacanciesProviderNames { get; internal set; }

        public string CountCaption => $"{"vacancy".ToQuantity(TransferredVacanciesCount)} {(TransferredVacanciesCount == 1 ? "has" : "have")} been transferred";
        public string ProviderNamesCaption => TransferredVacanciesProviderNames.Humanize().RemoveOxfordComma();
        public bool HasTransfersFromMultipleProviders => TransferredVacanciesProviderNames.Count() > 1;
    }
}
