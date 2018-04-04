using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.Preview;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class VacancyPreviewController : Controller
    {
        private readonly VacancyPreviewOrchestrator _orchestrator;

        public VacancyPreviewController(VacancyPreviewOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("vacancy-preview", Name = RouteNames.Preview_Index_Get)]
        public async Task<IActionResult> VacancyPreview(Guid vacancyId)
        {
            var vm = await _orchestrator.GetVacancyPreviewViewModelAsync(vacancyId);
            return View(vm);
        }

        [HttpPost("vacancy-submit", Name = RouteNames.Preview_Submit_Post)]
        public async Task<IActionResult> Submit(SubmitEditModel m)
        {
            var result = await _orchestrator.TrySubmitVacancyAsync(m);

            if (result)
            {
                return RedirectToRoute(RouteNames.Submitted_Index_Get);
            }
            
            ModelState.AddModelError(string.Empty, "Vacancy has already been submitted");
            var vm = await _orchestrator.GetVacancyPreviewViewModelAsync(m.VacancyId);
            return View("Index", vm);
        }
    }
}