using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.Preview;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId:minlength(6)}/vacancies/{vacancyId:guid}")]
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

        [HttpPost("vacancy-submit", Name = RouteNames.Preview_Submit_Post)]
        public async Task<IActionResult> Submit(SubmitEditModel m)
        {
            var result = await _orchestrator.TrySubmitVacancyAsync(m);

            if(result)
            {
                return RedirectToRoute(RouteNames.Submitted_Index_Get);
            }
            
            ModelState.AddModelError(string.Empty, "Vacancy has already been submitted");
            var vm = await _orchestrator.GetIndexViewModelAsync(m.VacancyId);
            return View("Index", vm);
        }
    }
}