﻿using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class EmployerContactDetailsController : Controller
    {
        private readonly EmployerContactDetailsOrchestrator _orchestrator;
        private readonly IFeature _feature;

        public EmployerContactDetailsController(EmployerContactDetailsOrchestrator orchestrator, IFeature feature)
        {
            _orchestrator = orchestrator;
            _feature = feature;
        }

        [HttpGet("employer-contact-details", Name = RouteNames.EmployerContactDetails_Get)]
        public async Task<IActionResult> EmployerContactDetails(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetEmployerContactDetailsViewModelAsync(vrm);
            return View(vm);
        }

        [HttpPost("employer-contact-details", Name = RouteNames.EmployerContactDetails_Post)]
        public async Task<IActionResult> EmployerContactDetails(EmployerContactDetailsEditModel m)
        {            
            var response = await _orchestrator.PostEmployerContactDetailsEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            var vm = await _orchestrator.GetEmployerContactDetailsViewModelAsync(m);
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            if (vm.IsTaskListCompleted)
            {
                return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new {m.VacancyId, m.EmployerAccountId});
            }
            return RedirectToRoute(RouteNames.ApplicationProcess_Get, new {m.VacancyId, m.EmployerAccountId});
        }
    }
}