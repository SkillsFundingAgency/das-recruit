using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Training;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class TrainingController : Controller
    {
        private readonly TrainingOrchestrator _orchestrator;

        public TrainingController(TrainingOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }
        
        [HttpGet("training", Name = RouteNames.Training_Get)]
        public async Task<IActionResult> Training(Guid vacancyId)
        {
            var vm = await _orchestrator.GetTrainingViewModelAsync(vacancyId);
            return View(vm);
        }

        [HttpPost("training", Name = RouteNames.Training_Post)]
        public async Task<IActionResult> Training(TrainingEditModel m)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetTrainingViewModelAsync(m);

                return View(vm);
            }

            await _orchestrator.PostTrainingEditModelAsync(m);

            return RedirectToRoute(RouteNames.Wage_Get);
        }
        
    }
}