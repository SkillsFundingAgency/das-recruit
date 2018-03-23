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
            /*
            If custom skills have been added via javascript then m.Skills could contain multiple invalid entries.
            As we only have one custom skills textbox we can only fix one at a time. 
            I think the best we can do is map the first erroring custom skill to the AddCustomSkillName input and 
            add the error to the AddCustomSkillName ModelState. We then remove this skill from the CustomSkills list and the 
            error from the Skills ModelState.
            The next invalid custom skill will be picked up the next time we try and submit the page and we repeat.

            An alternative is we also replicate the validation in javascript and validate on the client however this adds to the burden.
            Another alternative is we ajax the validation to be done on the server. However this latency will impact on UX.
            */

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetSkillsViewModelAsync(m);

                return View(vm);
            }

            await _orchestrator.PostSkillsEditModelAsync(m);

            if (!string.IsNullOrWhiteSpace(m.AddCustomSkillAction) || !string.IsNullOrWhiteSpace(m.RemoveCustomSkill))
            {
                return RedirectToRoute(RouteNames.Skills_Get);
            }

            return RedirectToRoute(RouteNames.Preview_Index_Get);
        }
    }
}