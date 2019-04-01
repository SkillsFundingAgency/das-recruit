using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Location
{
    public class LocationViewModel : VacancyRouteModel
    {
        public string LegalEntityLocation { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string Postcode { get; set; }
        public bool? UseOtherLocation { get; set; }
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(AddressLine1),
            nameof(AddressLine2),
            nameof(AddressLine3),
            nameof(AddressLine4),
            nameof(Postcode)
        };
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
        public PartOnePageInfoViewModel PageInfo { get; set; }
        public string CurrentLocation => 
            string
                .Join(", ", new[] {AddressLine1, AddressLine2, AddressLine3, AddressLine4, Postcode })
                .Replace(" ,", string.Empty);

        public bool CanShowBackLink { get; internal set; }
        public IList<string> LocationList { get; set; }
    }
}