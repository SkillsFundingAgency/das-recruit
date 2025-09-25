using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.MultipleLocations;

public class LocationAvailabilityViewModel : VacancyRouteModel
{
    public string ApprenticeshipTitle { get; init; }
    public PartOnePageInfoViewModel PageInfo { get; init; }
    public ReviewSummaryViewModel Review { get; set; } = new ();
    public AvailableWhere? SelectedAvailability { get; init; }
}