using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class TraineeshipSectorController : Controller
    {
        private readonly TraineeSectorOrchestrator _orchestrator;

        public TraineeshipSectorController(TraineeSectorOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }
        [HttpGet("traineeship-sector", Name = RouteNames.TraineeSector_Get)]
        public async Task<IActionResult> TraineeshipSector(VacancyRouteModel vrm, [FromQuery] string wizard = "true", [FromQuery] string clear = "", [FromQuery] int? routeId = null)
        {
            var clearTraining = string.IsNullOrWhiteSpace(clear) == false;

            var vm = await _orchestrator.GetTraineeViewModelAsync(vrm);

            if (routeId != null &&
                vm.Routes.Any(p => p.Id == routeId))
            {
                vm.SelectedRouteId = routeId.Value;
            }

            if (clearTraining)
                vm.SelectedRouteId = null;

            vm.PageInfo.SetWizard(wizard);

            return View(vm);
        }
    }
}