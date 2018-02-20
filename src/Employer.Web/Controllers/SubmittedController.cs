using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.Submitted;
using Esfa.Recruit.Employer.Web.Configuration.Routes;
using Esfa.Recruit.Employer.Web.Orchestrators;
using System.Threading.Tasks;
using System;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId}/vacancies/{vacancyId}")]
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