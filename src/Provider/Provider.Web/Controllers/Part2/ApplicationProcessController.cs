using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.ApplicationProcess;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
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

            return RedirectToRoute(RouteNames.Vacancy_Preview_Get);
        }
    }
}