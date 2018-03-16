using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class WageController : Controller
    {
        private readonly WageOrchestrator _orchestrator;

        public WageController(WageOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }
        
        [HttpGet("wage", Name = RouteNames.Wage_Get)]
        public async Task<IActionResult> Wage(Guid vacancyId)
        {
            var vm = await _orchestrator.GetWageViewModelAsync(vacancyId);
            return View(vm);
        }

        [HttpPost("wage", Name = RouteNames.Wage_Get)]
        public async Task<IActionResult> Wage(WageEditModel m)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetWageViewModelAsync(m);

                return View(vm);
            }

            await _orchestrator.PostWageEditModelAsync(m);

            return RedirectToRoute(RouteNames.Title_Get);
        }
    }
}