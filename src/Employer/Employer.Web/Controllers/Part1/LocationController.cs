using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;

namespace Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class LocationController : Controller
    {
        private readonly LocationOrchestrator _orchestrator;
        public LocationController(LocationOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("location", Name = RouteNames.Location_Get)]
        public async Task<IActionResult> Location(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var vm = await _orchestrator.GetLocationViewModelAsync(vrm, User.ToVacancyUser());
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }
    }
}