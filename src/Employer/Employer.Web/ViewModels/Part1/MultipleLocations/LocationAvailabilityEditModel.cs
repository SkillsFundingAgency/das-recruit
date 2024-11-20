using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.MultipleLocations;

public class LocationAvailabilityEditModel : VacancyRouteModel
{
    public AvailableWhere? SelectedAvailability { get; init; }
}