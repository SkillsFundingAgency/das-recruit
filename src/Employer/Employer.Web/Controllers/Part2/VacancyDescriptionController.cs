using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class VacancyDescriptionController : Controller
    {
        private readonly VacancyDescriptionOrchestrator _orchestrator;

        public VacancyDescriptionController(VacancyDescriptionOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("vacancy-description", Name = RouteNames.VacancyDescription_Index_Get)]
        public async Task<IActionResult> VacancyDescription(Guid vacancyId)
        {
            var vm = await _orchestrator.GetVacancyDescriptionViewModelAsync(vacancyId);
            return View(vm);
        }

        [HttpPost("vacancy-description", Name =  RouteNames.VacancyDescription_Index_Post)]
        public async Task<IActionResult> VacancyDescription(VacancyDescriptionEditModel m)
        {
            if(!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetVacancyDescriptionViewModelAsync(m.VacancyId);
                return View(vm);
            }
            
            await _orchestrator.PostVacancyDescriptionEditModelAsync(m);

            return RedirectToRoute(RouteNames.Preview_Index_Get);
        }
    }
}