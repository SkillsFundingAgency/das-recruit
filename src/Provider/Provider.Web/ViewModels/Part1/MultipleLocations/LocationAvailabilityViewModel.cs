using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.MultipleLocations;

public class LocationAvailabilityViewModel : VacancyRouteModel
{
    public string ApprenticeshipTitle { get; init; }
    public bool CanShowBackLink { get; internal set; }
    public PartOnePageInfoViewModel PageInfo { get; init; }
    public AvailableWhere? SelectedAvailability { get; init; }
}