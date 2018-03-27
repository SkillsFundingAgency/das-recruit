using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class AboutEmployerController : Controller
    {
        private readonly AboutEmployerOrchestrator _orchestrator;

        public AboutEmployerController(AboutEmployerOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("about-employer", Name = RouteNames.AboutEmployer_Get)]
        public async Task<IActionResult> AboutEmployer(Guid vacancyId)
        {
            var vm = await _orchestrator.GetAboutEmployerViewModelAsync(vacancyId);
            return View(vm);
        }

        [HttpPost("about-employer", Name =  RouteNames.AboutEmployer_Post)]
        public async Task<IActionResult> AboutEmployer(AboutEmployerEditModel m)
        {
            if(!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetAboutEmployerViewModelAsync(m.VacancyId);
                return View(vm);
            }
            
            await _orchestrator.PostAboutEmployerEditModelAsync(m);

            return RedirectToRoute(RouteNames.Preview_Index_Get);
        }
    }
}