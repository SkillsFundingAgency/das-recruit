using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Location
{
    public class LocationEditModel : VacancyRouteModel, IAddress
    {
        [Required(ErrorMessage = ValidationMessages.LocationPreferenceMessages.SelectionRequired)]
        public string SelectedLocation { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string Postcode { get; set; }
    }
}