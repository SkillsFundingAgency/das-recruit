using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.DeleteVacancy;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId:minlength(6)}/vacancies/{vacancyId:guid}")]
    public class DeleteVacancyController : Controller
    {
        private readonly DeleteVacancyOrchestrator _orchestrator;

        public DeleteVacancyController(DeleteVacancyOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("delete", Name = RouteNames.DeleteVacancy_Delete_Get)]
        public async Task<IActionResult> Delete(Guid vacancyId)
        {
            var vm = await _orchestrator.GetDeleteViewModelAsync(vacancyId);
            return View(vm);
        }

        [HttpPost("delete", Name = RouteNames.DeleteVacancy_Delete_Post)]
        public async Task<IActionResult> Delete(DeleteEditModel m)
        {
            if (!m.ConfirmDeletion)
            {
                return RedirectToRoute(RouteNames.Sections_Index_Get);
            }

            var result = await _orchestrator.TryDeleteVacancyAsync(m);

            if (!result)
            {
                ModelState.AddModelError(string.Empty, "This vacancy does not exist or has already been deleted.");
                var vm = await _orchestrator.GetDeleteViewModelAsync(m.VacancyId);
                return View(vm);
            }
            
            return RedirectToRoute(RouteNames.Dashboard_Index_Get);
        }
    }
}
