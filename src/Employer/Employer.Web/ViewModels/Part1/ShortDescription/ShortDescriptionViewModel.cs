using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.Configuration.Routing;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.ShortDescription
{
    public class ShortDescriptionViewModel : ShortDescriptionEditModel
    {
        public VacancyRouteParameters CancelButtonRouteParameters { get; set; }
        public bool InWizardMode { get; set; }
        public string SubmitButtonText => InWizardMode ? "Save and Continue" : "Save and Preview";

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(ShortDescription)
        };
    }
}
