using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.RouteModel;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.MultipleLocations;

public class AddMoreThanOneLocationEditModel: VacancyRouteModel
{
    public List<string> SelectedLocations { get; set; } = [];
}