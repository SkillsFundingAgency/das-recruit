using System.Collections.Generic;
using System.Web;
using Esfa.Recruit.Shared.Web.Extensions;
using Humanizer;

namespace Esfa.Recruit.Employer.Web.ViewModels.Dashboard
{
    public class BlockedProviderAlertViewModel
    {
        public List<string> ClosedVacancies { get; set; }
        public List<string> BlockedProviderNames { get; internal set; }

        public string CountCaption => $"{"vacancy".ToQuantity(ClosedVacancies.Count)} {(ClosedVacancies.Count == 1 ? "has" : "have")} been closed.";

        public string BlockedProviderNamesCaption => BlockedProviderNames.Humanize().RemoveOxfordComma();

        public bool HasMultipleBlockedProviders => BlockedProviderNames.Count > 1;
    }
}
