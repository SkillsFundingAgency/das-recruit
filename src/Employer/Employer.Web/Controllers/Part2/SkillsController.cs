using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Skills;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
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
            // If adding/removing skill, tempdata will have the current draft list.
            var vm = await _orchestrator.GetSkillsViewModelAsync(vrm, TempData[TempDataKeys.Skills] as string[]);

            return View(vm);
        }

        [HttpPost("skills", Name = RouteNames.Skills_Post)]
        public async Task<IActionResult> Skills(VacancyRouteModel vrm, SkillsEditModel m)
        {
            var response = await _orchestrator.PostSkillsEditModelAsync(vrm, m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetSkillsViewModelAsync(vrm);

                return View(vm);
            }

            if (!string.IsNullOrEmpty(m.AddCustomSkillAction))
            {
                TempData[TempDataKeys.Skills] = m.Skills;
                return RedirectToRoute(RouteNames.Skills_Get);
            }

            return RedirectToRoute(RouteNames.Vacancy_Preview_Get);
        }
    }
}