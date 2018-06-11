using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class ApplicationProcessController : Controller
    {
        private readonly ApplicationProcessOrchestrator _orchestrator;

        public ApplicationProcessController(ApplicationProcessOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("application-process", Name = RouteNames.ApplicationProcess_Get)]
        public async Task<IActionResult> ApplicationProcess(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetApplicationProcessViewModelAsync(vrm);
            return View(vm);
        }

        [HttpPost("application-process", Name = RouteNames.ApplicationProcess_Post)]
        public async Task<IActionResult> ApplicationProcess(ApplicationProcessEditModel m)
        {            
            var response = await _orchestrator.PostApplicationProcessEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetApplicationProcessViewModelAsync(m);
                return View(vm);
            }

            return RedirectToRoute(RouteNames.Vacancy_Preview_Get, ApplicationProcessViewModel.PreviewSectionAnchor);
        }
    }
}