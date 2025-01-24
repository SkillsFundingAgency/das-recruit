using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Models.AddLocation;

public class ConfirmLocationsEditModel : VacancyRouteModel
{
    [FromQuery] public bool Wizard { get; init; }
}