using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.CreateVacancyOptions;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountRoutePath)]
    public class CreateVacancyOptionsController : Controller
    {
        private readonly CreateVacancyOptionsOrchestrator _orchestrator;

        public CreateVacancyOptionsController(CreateVacancyOptionsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("create-options", Name = RouteNames.CreateVacancyOptions_Get)]
        public async Task<IActionResult> Options([FromRoute]string employerAccountId)
        {
            var vm = await _orchestrator.GetCreateOptionsViewModelAsync(employerAccountId);

            vm.ShowReturnToMALink = Convert.ToBoolean(TempData.Peek(TempDataKeys.ReferredFromMAHome));

            if (vm.HasClonableVacancies == false)
                return RedirectToRoute(RouteNames.CreateVacancy_Get);
            
            return View(vm);
        }

        [HttpPost("create-options", Name = RouteNames.CreateVacancyOptions_Post)]
        public async Task<IActionResult> Options([FromRoute]string employerAccountId, CreateVacancyOptionsEditModel model)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetCreateOptionsViewModelAsync(employerAccountId);
                return View(vm);
            }

            if (model.VacancyId == Guid.Empty)
                return RedirectToRoute(RouteNames.CreateVacancy_Get);

            var newVacancyId = await _orchestrator.CloneVacancy(employerAccountId, model.VacancyId.Value, User.ToVacancyUser());

            return RedirectToRoute(RouteNames.Vacancy_Preview_Get, new { VacancyId = newVacancyId });
        }
    }
}
