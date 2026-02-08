using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.Extensions;
using Humanizer;

namespace Esfa.Recruit.Shared.Web.ViewModels.Alerts
{
    public class EmployerTransferredVacanciesAlertViewModel
    {
        public int TransferredVacanciesCount { get; init; }
        public List<string> TransferredVacanciesProviderNames { get; init; }
        public string CountCaption => GetCaptionCount();
        private string GetCaptionCount() => $"{"advert".ToQuantity(TransferredVacanciesCount)} have been transferred";
        public string ProviderNamesCaption => TransferredVacanciesProviderNames.Humanize().RemoveOxfordComma();
    }
}