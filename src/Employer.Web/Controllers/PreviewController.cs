using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.Preview;
using Esfa.Recruit.Employer.Web.Configuration.Routes;
using Esfa.Recruit.Employer.Web.Orchestrators;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId}/vacancies/{vacancyId}")]
    public class PreviewController : Controller
    {
        private readonly PreviewOrchestrator _orchestrator;

        public PreviewController(PreviewOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("vacancy-preview", Name = RouteNames.Preview_Index_Get)]
        public async Task<IActionResult> Index(Guid vacancyId)
        {
            var vm = await _orchestrator.GetIndexViewModelAsync(vacancyId);
            return View(vm);
        }

        [HttpPost("vacancy-preview", Name = RouteNames.Preview_Index_Post)]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToRoute(RouteNames.Submitted_Index_Get);
        }
    }
}