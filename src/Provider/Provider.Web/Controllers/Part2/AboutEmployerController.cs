﻿using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.AboutEmployer;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class AboutEmployerController : Controller
    {
        private readonly AboutEmployerOrchestrator _orchestrator;

        public AboutEmployerController(AboutEmployerOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("about-employer", Name = RouteNames.AboutEmployer_Get)]
        public async Task<IActionResult> AboutEmployer(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetAboutEmployerViewModelAsync(vrm);
            return View(vm);
        }

        [HttpPost("about-employer", Name =  RouteNames.AboutEmployer_Post)]
        public async Task<IActionResult> AboutEmployer(AboutEmployerEditModel m)
        {
            var response = await _orchestrator.PostAboutEmployerEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            var vm = await _orchestrator.GetAboutEmployerViewModelAsync(m);
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            return vm.IsTaskListCompleted
                ? RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new { m.Ukprn, m.VacancyId })
                : RedirectToRoute(RouteNames.ProviderContactDetails_Get, new { m.Ukprn, m.VacancyId });
        }
    }
}