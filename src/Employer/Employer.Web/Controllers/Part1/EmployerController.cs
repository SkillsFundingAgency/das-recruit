using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route("accounts/{employerAccountId}/vacancies/{vacancyId}")]
    public class EmployerController : Controller
    {
        private readonly EmployerOrchestrator _orchestrator;

        public EmployerController(EmployerOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("employer", Name = RouteNames.Employer_Get)]
        public async Task<IActionResult> Employer(Guid vacancyId)
        {
            var vm = await _orchestrator.GetLocationViewModelAsync(vacancyId);
            return View(vm);
        }

        [HttpPost("employer", Name = RouteNames.Employer_Post)]
        public async Task<IActionResult> Employer(EmployerEditModel m)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetLocationViewModelAsync(m);

                return View(vm);
            }

            await _orchestrator.PostLocationEditModelAsync(m);
            return RedirectToRoute(RouteNames.ShortDescription_Get);
        }
        
    }
}