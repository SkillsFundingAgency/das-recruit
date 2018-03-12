using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId:minlength(6)}/vacancies/{vacancyId:guid}")]
    public class SubmittedController : Controller
    {
        private readonly SubmittedOrchestrator _orchestrator;

        public SubmittedController(SubmittedOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("vacancy-submitted", Name = RouteNames.Submitted_Index_Get)]
        public async Task<IActionResult> Index(Guid vacancyId)
        {
            var vm = await _orchestrator.GetIndexViewModelAsync(vacancyId);
            
            return View(vm);
        }
    }
}