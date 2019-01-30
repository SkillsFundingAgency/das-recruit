using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Employer;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Shared.Web.Extensions;
using Provider.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountRoutePath)]
    public class EmployerController : Controller
    {
        private readonly EmployerOrchestrator _orchestrator;

        public EmployerController(EmployerOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("employer", Name = RouteNames.Employer_Get)]
        public async Task<IActionResult> Employer(VacancyRouteModel vacancyRouteModel, [FromQuery] string wizard = "true")
        {
            var vm = await _orchestrator.GetEmployersViewModelAsync(vacancyRouteModel);
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost("employer", Name = RouteNames.Employer_Post)]
        public async Task<IActionResult> Employer(VacancyRouteModel vacancyRouteModel, 
            EmployersEditModel model, [FromQuery] bool wizard)
        {            
            if (string.IsNullOrWhiteSpace(model.SelectedEmployerId))
            {
                ModelState.AddModelError(nameof(model.SelectedEmployerId), ValidationMessages.EmployerSelectionMessages.EmployerMustBeSelectedMessage);
            }
            
            if(!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetEmployersViewModelAsync(model);
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }
            
            return
                wizard 
                    ? RedirectToRoute(RouteNames.CreateVacancy_Get, new {employerAccountId = model.SelectedEmployerId}) 
                    : RedirectToRoute(RouteNames.Vacancy_Preview_Get);
        }
    }
}