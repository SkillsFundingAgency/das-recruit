using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Location
{
    public class LocationViewModel : VacancyRouteModel
    {
        public const string UseOtherLocationConst = "UseOtherLocation";
        public string UseOtherLocation => UseOtherLocationConst;
        public string SelectedLocation { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string Postcode { get; set; }

        public bool IsAnonymousVacancy { get; set; }

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
        public bool CanShowBackLink { get; internal set; }
        public List<string> AvailableLocations { get; set; } = new List<string>();
        public void SetLocation(Address location)
        {
            AddressLine1 = location.AddressLine1;
            AddressLine2 = location.AddressLine2;
            AddressLine3 = location.AddressLine3;
            AddressLine4 = location.AddressLine4;
            Postcode = location.Postcode;
        }
    }
}