using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.NumberOfPositions
{
    public class NumberOfPositionsViewModel : NumberOfPositionsEditModel
    {
        public PartOnePageInfoViewModel PageInfo { get; set; }
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(NumberOfPositions)
        };
    }
}
