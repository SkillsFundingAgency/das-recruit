using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Employer;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Shared.Web.Extensions;

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

        [HttpGet("create-vacancy", Name = RouteNames.CreateVacancy_Get)]
        public async Task<IActionResult> Employer(VacancyRouteModel vacancyRouteModel, [FromQuery] string wizard = "true")
        {
            var vm = await _orchestrator.GetEmployersViewModelAsync(vacancyRouteModel);
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost("create-vacancy", Name = RouteNames.CreateVacancy_Post)]
        public async Task<IActionResult> Employer(VacancyRouteModel vacancyRouteModel, 
            EmployersEditViewModel viewModel, [FromQuery] bool wizard)
        {            
            var response = await _orchestrator.PostEmployerEditModelAsync(vacancyRouteModel, viewModel, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if(!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetEmployersViewModelAsync(viewModel);
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }
            
            return
                wizard 
                    ? RedirectToRoute(RouteNames.Title_Get, new {vacancyId = response.Data}) 
                    : RedirectToRoute(RouteNames.Vacancy_Preview_Get);
        }
    }
}