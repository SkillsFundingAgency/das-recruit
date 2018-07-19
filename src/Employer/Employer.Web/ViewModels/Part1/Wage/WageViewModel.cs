using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage
{
    public class WageViewModel : WageEditModel
    {
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(WageEditModel.Duration),
            nameof(WageEditModel.WorkingWeekDescription),
            nameof(WageEditModel.WeeklyHours),
            nameof(WageEditModel.WageType),
            nameof(WageEditModel.FixedWageYearlyAmount),
            nameof(WageEditModel.WageAdditionalInformation)
        };

        public bool IsWizard { get; set; }
        public bool IsNotWizard => !IsWizard;
        public string SubmitButtonText => IsWizard ? "Save and Continue" : "Save and Preview";
    }
}
