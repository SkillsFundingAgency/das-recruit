using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.MultipleLocations;

public class LocationAvailabilityEditModel : VacancyRouteModel
{
    [Required(ErrorMessage = ValidationMessages.MultipleLocationMessages.SelectionRequired)]
    public AvailableWhere? SelectedAvailability { get; init; }
}