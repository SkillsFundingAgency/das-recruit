using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class ConsiderationsController : Controller
    {
        private readonly ConsiderationsOrchestrator _orchestrator;

        public ConsiderationsController(ConsiderationsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("considerations", Name = RouteNames.Considerations_Get)]
        public async Task<IActionResult> Considerations(Guid vacancyId)
        {
            var vm = await _orchestrator.GetConsiderationsViewModelAsync(vacancyId);
            return View(vm);
        }

        [HttpPost("considerations", Name =  RouteNames.Considerations_Post)]
        public async Task<IActionResult> Considerations(ConsiderationsEditModel m)
        {
            if(!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetConsiderationsViewModelAsync(m.VacancyId);
                return View(vm);
            }
            
            await _orchestrator.PostConsiderationsEditModelAsync(m);

            return RedirectToRoute(RouteNames.Preview_Index_Get);
        }
    }
}