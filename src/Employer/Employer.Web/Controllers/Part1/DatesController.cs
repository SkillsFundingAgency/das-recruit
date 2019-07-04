using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Dates;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class DatesController : Controller
    {
        private readonly DatesOrchestrator _orchestrator;

        public DatesController(DatesOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }
        
        [HttpGet("dates", Name = RouteNames.Dates_Get)]
        public async Task<IActionResult> Dates(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var vm = await _orchestrator.GetDatesViewModelAsync(vrm);
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost("dates", Name = RouteNames.Dates_Post)]
        public async Task<IActionResult> Dates(DatesEditModel m, [FromQuery] bool wizard)
        {
            var response = await _orchestrator.PostDatesEditModelAsync(m, User.ToVacancyUser());
            
            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetDatesViewModelAsync(m);
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }

            return wizard
                ? RedirectToRoute(RouteNames.Wage_Get)
                : RedirectToRoute(RouteNames.Vacancy_Preview_Get);
        }            
    }
}