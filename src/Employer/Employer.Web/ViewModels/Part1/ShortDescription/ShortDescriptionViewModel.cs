using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.ShortDescription
{
    public class ShortDescriptionViewModel : ShortDescriptionEditModel
    {
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(ShortDescription)
        };

        public bool IsWizard { get; set; }
        public bool IsNotWizard => !IsWizard;
        public string SubmitButtonText => IsWizard ? "Save and Continue" : "Save and Preview";
    }
}
