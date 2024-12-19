using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.AddLocation;

public class SelectLocationViewModel : VacancyRouteModel
{
    public string ApprenticeshipTitle { get; init; }
    public MultipleLocationsJourneyOrigin Origin { get; init; }
    public IEnumerable<string> Locations { get; init; }
    public PartOnePageInfoViewModel PageInfo { get; init; }
    public string Postcode { get; init; }
    public string SelectedLocation { get; init; }
}