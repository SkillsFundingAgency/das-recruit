using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage
{
    public class WageViewModel : WageEditModel
    {
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(Duration),
            nameof(WorkingWeekDescription),
            nameof(WeeklyHours),
            nameof(WageType),
            nameof(FixedWageYearlyAmount),
            nameof(WageAdditionalInformation)
        };

        public PartOnePageInfoViewModel PageInfo { get; set; }
    }
}
