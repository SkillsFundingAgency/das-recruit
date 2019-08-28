using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Duration;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class DurationController : Controller
    {
        private readonly DurationOrchestrator _orchestrator;

        public DurationController(DurationOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }
        
        [HttpGet("duration", Name = RouteNames.Duration_Get)]
        public async Task<IActionResult> Duration(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var vm = await _orchestrator.GetDurationViewModelAsync(vrm);
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost("duration", Name = RouteNames.Duration_Post)]
        public async Task<IActionResult> Duration(DurationEditModel m, [FromQuery] bool wizard)
        {
            var response = await _orchestrator.PostDurationEditModelAsync(m, User.ToVacancyUser());
            
            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetDurationViewModelAsync(m);
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }

            return wizard
                ? RedirectToRoute(RouteNames.Wage_Get)
                : RedirectToRoute(RouteNames.Vacancy_Preview_Get);
        }
    }
}