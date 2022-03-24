using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.Extensions;
using Humanizer;

namespace Esfa.Recruit.Shared.Web.ViewModels.Alerts
{
    public class ProviderTransferredVacanciesAlertViewModel
    {
        public List<string> LegalEntityNames { get; internal set; }

        public string LegalEntityNamesCaption => LegalEntityNames.Humanize().RemoveOxfordComma();

        public bool HasTransfersToMultipleEmployers => LegalEntityNames.Count() > 1;
        public long Ukprn { get; set; }
    }
}
