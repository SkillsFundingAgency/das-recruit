using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId}/vacancies/{vacancyId}")]
    public class SectionsController : Controller
    {
        private readonly SectionsOrchestrator _orchestrator;

        public SectionsController(SectionsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("sections", Name = RouteNames.Sections_Index_Get)]
        public async Task<IActionResult> Index(Guid vacancyId)
        {
            var vm = await _orchestrator.GetIndexViewModelAsync(vacancyId);

            return View(vm);
        }
    }
}