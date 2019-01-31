using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Location
{
    public class LocationViewModel
    {
        public IEnumerable<LegalEntityViewModel> LegalEntities { get; set; }

        public bool HasOnlyOneOrganisation => LegalEntities.Count() == 1;
		public bool HasMoreThanOneOrganisation => LegalEntities.Count() > 1;

        //public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(AddressLine1),
            nameof(AddressLine2),
            nameof(AddressLine3),
            nameof(AddressLine4),
            nameof(Postcode)
        };

        public PartOnePageInfoViewModel PageInfo { get; set; }
        public long SelectedLegalEntityId { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string AddressLine3 { get; set; }

        public string AddressLine4 { get; set; }

        public string Postcode { get; set; }
    }
}