using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
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
        public async Task<IActionResult> Skills(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetSkillsViewModelAsync(vrm);

            TryUpdateSkillsFromTempData(vm);
            
            return View(vm);
        }

        [HttpPost("skills", Name = RouteNames.Skills_Post)]
        public async Task<IActionResult> Skills(SkillsEditModel m)
        {
            var response = await _orchestrator.PostSkillsEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetSkillsViewModelAsync(m);

                return View(vm);
            }

            if (m.IsAddingCustomSkill || m.IsRemovingCustomSkill)
            {
                TempData[TempDataKeys.Skills] = m.Skills;
                return RedirectToRoute(RouteNames.Skills_Get);
            }

            return RedirectToRoute(RouteNames.Vacancy_Preview_Get, SkillsViewModel.PreviewSectionAnchor);
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