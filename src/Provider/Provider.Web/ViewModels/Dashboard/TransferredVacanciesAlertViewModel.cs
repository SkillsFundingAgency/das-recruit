﻿using System.Collections.Generic;
using System.Linq;
using System.Web;
using Esfa.Recruit.Shared.Web.Extensions;
using Humanizer;

namespace Esfa.Recruit.Provider.Web.ViewModels.Dashboard
{
    public class TransferredVacanciesAlertViewModel
    {
        public IEnumerable<string> LegalEntityNames { get; internal set; }

        public string LegalEntityNamesCaptionRaw => LegalEntityNames.Humanize(s => $"<span class=\"govuk-!-font-weight-bold\">{HttpUtility.HtmlEncode(s)}</span>").RemoveOxfordComma();

        public bool HasTransfersToMultipleEmployers => LegalEntityNames.Count() > 1;
    }
}
