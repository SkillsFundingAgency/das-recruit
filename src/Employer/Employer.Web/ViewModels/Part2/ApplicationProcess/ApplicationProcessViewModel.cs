using Esfa.Recruit.Employer.Web.Views;
﻿using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class ApplicationProcessViewModel
    {
        public string Title { get; internal set; }
        public ApplicationMethod? ApplicationMethod { get; internal set; }
        public string ApplicationInstructions { get; internal set; }
        public string ApplicationUrl { get; internal set; }

        public bool HasEmptyApplicationMethod => !ApplicationMethod.HasValue;

        public static string PreviewSectionAnchor => PreviewAnchors.ApplicationProcessSection;

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(ApplicationProcessEditModel.ApplicationMethod),
            nameof(ApplicationProcessEditModel.ApplicationUrl),
            nameof(ApplicationProcessEditModel.ApplicationInstructions)
        };

        public string FindAnApprenticeshipUrl { get; internal set; }
    }
}
