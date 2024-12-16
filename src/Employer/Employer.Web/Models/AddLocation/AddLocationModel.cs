using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Models.AddLocation;

public class AddLocationModel : VacancyRouteModel
{
    [FromQuery] public MultipleLocationsJourneyOrigin Origin { get; set; }
    [FromQuery] public bool Wizard { get; set; } = true;
}