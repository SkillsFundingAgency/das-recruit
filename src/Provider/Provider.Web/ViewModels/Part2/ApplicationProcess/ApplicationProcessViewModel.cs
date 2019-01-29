using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part2.ApplicationProcess
{
    public class ApplicationProcessViewModel
    {
        public string Title { get; internal set; }
        public ApplicationMethod? ApplicationMethod { get; internal set; }
        public string ApplicationInstructions { get; internal set; }
        public string ApplicationUrl { get; internal set; }

        public bool HasEmptyApplicationMethod => !ApplicationMethod.HasValue;
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(ApplicationMethod),
            nameof(ApplicationUrl),
            nameof(ApplicationInstructions)
        };

        public string FindAnApprenticeshipUrl { get; internal set; }
    }
}
