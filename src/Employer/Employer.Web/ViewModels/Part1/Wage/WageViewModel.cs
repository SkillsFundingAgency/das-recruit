using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage
{
    public class WageViewModel : WageEditModel
    {
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(WageEditModel.Duration),
            nameof(WageEditModel.WeeklyHours),
            nameof(WageEditModel.WageType),
            nameof(WageEditModel.FixedWageYearlyAmount),
            nameof(WageEditModel.WageAdditionalInformation)
        };
    }
}
