using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Location;

public class AddOneLocationEditModel: VacancyRouteModel
{
    public ReviewSummaryViewModel Review { get; set; } = new ();
    public string SelectedLocation { get; set; }
}