using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer
{
    public class EmployerViewModel : EmployerEditModel
    {
        public IEnumerable<LocationOrganisationViewModel> Organisations { get; set; }

        public bool HasOnlyOneOrganisation => Organisations.Count() == 1;
        
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(EmployerEditModel.AddressLine1),
            nameof(EmployerEditModel.AddressLine2),
            nameof(EmployerEditModel.AddressLine3),
            nameof(EmployerEditModel.AddressLine4),
            nameof(EmployerEditModel.Postcode)
        };

        public PartOnePageInfoViewModel PageInfo { get; set; }
    }

    public class LocationOrganisationViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Address Address { get; internal set; }
    }
}
