using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels.Location
{
    public class LocationViewModel : LocationEditModel
    {
        public IEnumerable<LocationOrganisationViewModel> Organisations { get; set; }
    }

    public class LocationOrganisationViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
