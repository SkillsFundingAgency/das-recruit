using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.ShortDescription
{
    public class ShortDescriptionViewModel : ShortDescriptionEditModel
    {
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(ShortDescription)
        };

        public PartOnePageInfoViewModel PageInfo { get; set; }
    }
}
