using Esfa.Recruit.Employer.Web.Domain;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.AddLocation;

public class AddLocationViewModel : VacancyRouteModel
{
    public string ApprenticeshipTitle { get; init; }
    public PartOnePageInfoViewModel PageInfo { get; init; }
    public string Postcode { get; init; }
    public MultipleLocationsJourneyOrigin Origin { get; init; }
}