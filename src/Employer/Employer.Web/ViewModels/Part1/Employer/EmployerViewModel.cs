using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer
{
    public class EmployerViewModel : EmployerEditModel
    {
        public IEnumerable<LocationOrganisationViewModel> Organisations { get; set; }

        public bool HasOnlyOneOrganisation => Organisations.Count() == 1;
		public bool HasMoreThanOneOrganisation => Organisations.Count() > 1;

        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(AddressLine1),
            nameof(AddressLine2),
            nameof(AddressLine3),
            nameof(AddressLine4),
            nameof(Postcode)
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
