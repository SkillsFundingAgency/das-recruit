using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Location
{
    public class LocationEditModel : VacancyRouteModel
    {
        [Required(ErrorMessage = ValidationMessages.LocationPreferenceMessages.SelectionRequired)]
        public string SelectedLocation { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string Postcode { get; set; }
        public List<string> AvailableLocations { get; set; } = new List<string>();
        public string OtherLocationString =>  
            string
                .Join(", ", new[] {AddressLine1, AddressLine2, AddressLine3, AddressLine4, Postcode })
                .Replace(" ,", string.Empty);
    }
}