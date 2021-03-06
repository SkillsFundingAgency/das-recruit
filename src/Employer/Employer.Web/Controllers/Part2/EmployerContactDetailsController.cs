﻿using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class EmployerContactDetailsController : Controller
    {
        private readonly EmployerContactDetailsOrchestrator _orchestrator;

        public EmployerContactDetailsController(EmployerContactDetailsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
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

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetEmployerContactDetailsViewModelAsync(m);
                return View(vm);
            }

            return RedirectToRoute(RouteNames.Vacancy_Preview_Get);
        }
    }
}