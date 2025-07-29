﻿using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.CustomWage;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class CustomWageController(CustomWageOrchestrator orchestrator) : Controller
    {
        [HttpGet("custom-wage", Name = RouteNames.CustomWage_Get)]
        public async Task<IActionResult> CustomWage(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var vm = await orchestrator.GetCustomWageViewModelAsync(vrm);
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost("custom-wage", Name = RouteNames.CustomWage_Post)]
        public async Task<IActionResult> CustomWage(CustomWageEditModel m, [FromQuery] bool wizard)
        {
            var response = await orchestrator.PostCustomWageEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await orchestrator.GetCustomWageViewModelAsync(m);
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }

            return RedirectToRoute(RouteNames.AddExtraInformation_Get, new { m.VacancyId, m.EmployerAccountId, wizard });
        }
    }
}
