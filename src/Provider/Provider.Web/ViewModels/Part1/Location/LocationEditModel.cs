using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Provider.Web.RouteModel;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Location
{
    public class LocationEditModel : VacancyRouteModel
    {
        [Required(ErrorMessage = ValidationMessages.LocationPreferenceMessages.SelectionRequired)]
        public bool? UseOtherLocation { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string AddressLine3 { get; set; }

        public string AddressLine4 { get; set; }

        public string Postcode { get; set; }
    }
}