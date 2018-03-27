using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class QualificationsController : Controller
    {
        private readonly QualificationsOrchestrator _orchestrator;

        public QualificationsController(QualificationsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("qualifications", Name = RouteNames.Qualifications_Get)]
        public async Task<IActionResult> Qualifications(Guid vacancyId)
        {
            var vm = await _orchestrator.GetQualificationsViewModelAsync(vacancyId);
            return View(vm);
        }

        [HttpPost("qualifications", Name = RouteNames.Qualifications_Post)]
        public async Task<IActionResult> Qualifications(QualificationsEditModel m)
        {
            
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetQualificationsViewModelAsync(m);
                
                return View(vm);
            }

            await _orchestrator.PostQualificationsEditModelAsync(m);
            
            return RedirectToRoute(RouteNames.Qualifications_Get);
        }
    }
}