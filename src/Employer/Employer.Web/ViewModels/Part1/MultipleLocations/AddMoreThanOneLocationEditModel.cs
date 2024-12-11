using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.MultipleLocations;

public class AddMoreThanOneLocationEditModel: VacancyRouteModel
{
    public List<string> SelectedLocations { get; set; } = [];
}