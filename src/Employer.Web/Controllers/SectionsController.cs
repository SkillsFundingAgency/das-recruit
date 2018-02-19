using Esfa.Recruit.Employer.Web;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.Sections;
using Esfa.Recruit.Employer.Web.Configuration.Routes;
using System;
using Esfa.Recruit.Employer.Web.Orchestrators;
using System.Threading.Tasks;

namespace Employer.Web.Controllers
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