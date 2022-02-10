﻿using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.ShortDescription
{
    public class ShortDescriptionViewModel : ShortDescriptionEditModel
    {
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(ShortDescription)
        };

        public PartOnePageInfoViewModel PageInfo { get; set; } = new PartOnePageInfoViewModel();
        public string Title { get; internal set; }
    }
}
