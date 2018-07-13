using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.Configuration.Routing;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer
{
    public class EmployerViewModel : EmployerEditModel
    {
        public IEnumerable<LocationOrganisationViewModel> Organisations { get; set; }
        public bool HasOnlyOneOrganisation => Organisations.Count() == 1;

        public VacancyRouteParameters CancelButtonRouteParameters { get; set; }
        public bool InWizardMode { get; set; }
        public string SubmitButtonText => InWizardMode ? "Save and Continue" : "Save and Preview";

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(AddressLine1),
            nameof(AddressLine2),
            nameof(AddressLine3),
            nameof(AddressLine4),
            nameof(Postcode)
        };
    }

    public class LocationOrganisationViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
