using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Skills;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.FeatureToggle;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class SkillsController : Controller
    {
        private readonly SkillsOrchestrator _orchestrator;
        private readonly IFeature _feature;

        public SkillsController(SkillsOrchestrator orchestrator, IFeature feature)
        {
            _orchestrator = orchestrator;
            _feature = feature;
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

            var vm = await _orchestrator.GetSkillsViewModelAsync(vrm);
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            if (!string.IsNullOrEmpty(m.AddCustomSkillAction))
            {
                TempData[TempDataKeys.Skills] = m.Skills;
                return RedirectToRoute(RouteNames.Skills_Get);
            }

            if (_feature.IsFeatureEnabled(FeatureNames.EmployerTaskList))
            {
                if (vm.IsTaskListCompleted)
                {
                    return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet);
                }
                return RedirectToRoute(RouteNames.Qualifications_Get);
            }
            return RedirectToRoute(RouteNames.Vacancy_Preview_Get);
        }
    }
}