using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillsEditModel = Esfa.Recruit.Provider.Web.ViewModels.Part2.Skills.SkillsEditModel;

namespace Esfa.Recruit.Provider.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class SkillsController : Controller
    {
        private readonly SkillsOrchestrator _orchestrator;
        private readonly IFeature _feature;
        private readonly ServiceParameters _serviceParameters;

        public SkillsController(SkillsOrchestrator orchestrator, IFeature feature, ServiceParameters serviceParameters)
        {
            _orchestrator = orchestrator;
            _feature = feature;
            _serviceParameters = serviceParameters;
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

            var vm = await _orchestrator.GetSkillsViewModelAsync(vrm, m);
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            if (!string.IsNullOrEmpty(m.AddCustomSkillAction))
            {
                TempData[TempDataKeys.Skills] = m.Skills;
                return RedirectToRoute(RouteNames.Skills_Get, new {vrm.Ukprn, vrm.VacancyId});
            }

            if (_feature.IsFeatureEnabled(FeatureNames.ProviderTaskList))
            {
                if (!vm.IsTaskListCompleted)
                {
                    if (_serviceParameters.VacancyType == VacancyType.Apprenticeship)
                    {
                        return RedirectToRoute(RouteNames.Qualifications_Get, new {vrm.Ukprn, vrm.VacancyId});    
                    }
                    
                    return RedirectToRoute(RouteNames.FutureProspects_Get, new {vrm.Ukprn, vrm.VacancyId});
                }
                return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new {vrm.Ukprn, vrm.VacancyId});
                
            }

            return RedirectToRoute(RouteNames.Vacancy_Preview_Get, new {vrm.Ukprn, vrm.VacancyId});
        }
    }
}