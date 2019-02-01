using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Location;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1
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
            var vm = await _orchestrator.GetLocationViewModelAsync(vrm, User.GetUkprn());
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost("location", Name = RouteNames.Location_Post)]
        public async Task<IActionResult> Location(LocationEditModel m, [FromQuery] bool wizard)
        {
            var response = await _orchestrator.PostLocationEditModelAsync(m, User.ToVacancyUser(), User.GetUkprn());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetLocationViewModelAsync(m, User.GetUkprn());
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }

            return wizard
                ? RedirectToRoute(RouteNames.Training_Get)
                : RedirectToRoute(RouteNames.Vacancy_Preview_Get, null, Anchors.AboutEmployerSection);
        }

    }
}