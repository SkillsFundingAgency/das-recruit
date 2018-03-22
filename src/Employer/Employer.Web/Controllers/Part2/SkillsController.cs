using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Skills;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class SkillsController : Controller
    {
        private readonly SkillsOrchestrator _orchestrator;

        public SkillsController(SkillsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("skills", Name = RouteNames.Skills_Get)]
        public async Task<IActionResult> Skills(Guid vacancyId)
        {
            var vm = await _orchestrator.GetSkillsViewModelAsync(vacancyId);
            return View(vm);
        }

        [HttpPost("skills", Name = RouteNames.Skills_Post)]
        public async Task<IActionResult> Skills(SkillsEditModel m)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetSkillsViewModelAsync(m);

                return View(vm);
            }

            await _orchestrator.PostSkillsEditModelAsync(m);
            return RedirectToRoute(RouteNames.Skills_Get);
        }
    }
}