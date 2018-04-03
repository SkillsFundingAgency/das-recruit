using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
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

            TryUpdateSkillsFromTempData(vm);
            
            return View(vm);
        }

        [HttpPost("skills", Name = RouteNames.Skills_Post)]
        public async Task<IActionResult> Skills(SkillsEditModel m)
        {
            if (m.IsAddingCustomSkill || m.IsRemovingCustomSkill)
            {
                HandleCustomSkillChange(m);

                return RedirectToRoute(RouteNames.Skills_Get);
            }

            var response = await _orchestrator.PostSkillsEditModelAsync(m);

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetSkillsViewModelAsync(m);

                return View(vm);
            }
            
            return RedirectToRoute(RouteNames.Preview_Index_Get);
        }
        
        private void HandleCustomSkillChange(SkillsEditModel m)
        {
            var skills = m.Skills ?? new List<string>();

            if (m.IsAddingCustomSkill)
            {
                skills.Add(m.AddCustomSkillName);
            }

            if (m.IsRemovingCustomSkill)
            {
                skills.Remove(m.RemoveCustomSkill);
            }

            TempData.Add(TempDataKeys.Skills, skills);
        }
        
        private void TryUpdateSkillsFromTempData(SkillsViewModel vm)
        {
            if (TempData.ContainsKey(TempDataKeys.Skills))
            {
                var tempDataSkills = TempData[TempDataKeys.Skills] as string[] ?? new string[0];
                _orchestrator.SetViewModelSkills(vm, tempDataSkills);
            }
        }
    }
}