using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class EmployerController : Controller
    {
        private readonly EmployerOrchestrator _orchestrator;

        public EmployerController(EmployerOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("employer", Name = RouteNames.Employer_Get)]
        public async Task<IActionResult> Employer(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetEmployerViewModelAsync(vrm);
            return View(vm);
        }

        [HttpPost("employer", Name = RouteNames.Employer_Post)]
        public async Task<IActionResult> Employer(EmployerEditModel m)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetEmployerViewModelAsync(m);

                return View(vm);
            }

            await _orchestrator.PostEmployerEditModelAsync(m);
            return RedirectToRoute(RouteNames.ShortDescription_Get);
        }
    }
}