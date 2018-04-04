using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer
{
    public class EmployerViewModel : EmployerEditModel
    {
        public IEnumerable<LocationOrganisationViewModel> Organisations { get; set; }

        public bool HasOnlyOneOrganisation => Organisations.Count() == 1;
        
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(EmployerEditModel.AddressLine1),
            nameof(EmployerEditModel.Postcode)
        };
    }

    public class LocationOrganisationViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
