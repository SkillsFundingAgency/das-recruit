using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.MultipleLocations;

public class LocationAvailabilityViewModel : VacancyRouteModel
{
    public string ApprenticeshipTitle { get; init; }
    public PartOnePageInfoViewModel PageInfo { get; init; }
    public AvailableWhere? SelectedAvailability { get; init; }
}