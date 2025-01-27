using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Models.AddLocation;

public class AddLocationJourneyModel : VacancyRouteModel
{
    [FromQuery] public MultipleLocationsJourneyOrigin Origin { get; set; }
    [FromQuery] public bool Wizard { get; set; } = true;
}