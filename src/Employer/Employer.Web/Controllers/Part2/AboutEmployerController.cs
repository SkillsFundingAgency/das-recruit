﻿using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
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
            
            if (!vm.IsTaskListCompleted)
            {
                return RedirectToRoute(RouteNames.EmployerContactDetails_Get, new {m.VacancyId, m.EmployerAccountId});
            }
            return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new {m.VacancyId, m.EmployerAccountId});
        }
    }
}