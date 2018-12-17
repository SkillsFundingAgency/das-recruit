using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class ConsiderationsController : Controller
    {
        private readonly ConsiderationsOrchestrator _orchestrator;

        public ConsiderationsController(ConsiderationsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("considerations", Name = RouteNames.Considerations_Get)]
        public async Task<IActionResult> Considerations(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetConsiderationsViewModelAsync(vrm);
            return View(vm);
        }

        [HttpPost("considerations", Name =  RouteNames.Considerations_Post)]
        public async Task<IActionResult> Considerations(ConsiderationsEditModel m)
        {
            var response = await _orchestrator.PostConsiderationsEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetConsiderationsViewModelAsync(m);

                return View(vm);
            }

            return RedirectToRoute(RouteNames.Vacancy_Preview_Get);
        }
    }
}