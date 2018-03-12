using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels.Location;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId:minlength(6)}/vacancies/{vacancyId:guid:validGuid}")]
    public class LocationController : Controller
    {
        private readonly LocationOrchestrator _orchestrator;

        public LocationController(LocationOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("location", Name = RouteNames.Location_Get)]
        public async Task<IActionResult> Location(Guid vacancyId)
        {
            var vm = await _orchestrator.GetLocationViewModelAsync(vacancyId);
            return View(vm);
        }

        [HttpPost("location", Name = RouteNames.Location_Post)]
        public async Task<IActionResult> Location(LocationEditModel m)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetLocationViewModelAsync(m);

                return View(vm);
            }

            await _orchestrator.PostLocationEditModelAsync(m);
            return RedirectToRoute(RouteNames.Title_Get);
        }        
    }
}