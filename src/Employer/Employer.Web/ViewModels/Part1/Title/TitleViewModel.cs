using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Title
{
    public class TitleViewModel : TitleEditModel
    {
        public VacancyRouteParameters CancelButtonRouteParameters { get; set; }
        public bool InWizardMode { get; set; }
        public string SubmitButtonText => InWizardMode ? "Save and Continue" : "Save and Preview";

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(NumberOfPositions),
            nameof(Title)
        };
    }
}
