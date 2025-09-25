using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Location;

public class AddOneLocationEditModel: VacancyRouteModel
{
    public ReviewSummaryViewModel Review { get; set; } = new ();
    public string SelectedLocation { get; set; }
}