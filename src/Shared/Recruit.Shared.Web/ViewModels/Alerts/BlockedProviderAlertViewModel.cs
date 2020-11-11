using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.Extensions;
using Humanizer;

namespace Esfa.Recruit.Shared.Web.ViewModels.Alerts
{
    public class BlockedProviderAlertViewModel
    {
        public List<string> ClosedVacancies { get; set; }
        public List<string> BlockedProviderNames { get; internal set; }

        public string CountCaption => GetCaptionCount();

        public string BlockedProviderNamesCaption => BlockedProviderNames.Humanize().RemoveOxfordComma();

        public bool HasMultipleBlockedProviders => BlockedProviderNames.Count > 1;

        private string GetCaptionCount()
        {
            if (ClosedVacancies.Count == 1)
            {
                return $"{"advert".ToQuantity(ClosedVacancies.Count, ShowQuantityAs.Words).Transform(To.SentenceCase)} has been transferred";
            }
            return $"{"advert".ToQuantity(ClosedVacancies.Count)} have been transferred";
        }
    }
}
