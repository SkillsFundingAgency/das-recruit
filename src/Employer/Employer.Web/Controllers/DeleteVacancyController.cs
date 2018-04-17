using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.DeleteVacancy;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class DeleteVacancyController : Controller
    {
        private readonly DeleteVacancyOrchestrator _orchestrator;

        public DeleteVacancyController(DeleteVacancyOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("delete", Name = RouteNames.DeleteVacancy_Delete_Get)]
        public Task<IActionResult> Delete(Guid vacancyId)
        {
            return GetDeleteVacancyConfirmationView(vacancyId);
        }

        [HttpPost("delete", Name = RouteNames.DeleteVacancy_Delete_Post)]
        public async Task<IActionResult> Delete(DeleteEditModel m)
        {
            if (!ModelState.IsValid)
            {
                return await GetDeleteVacancyConfirmationView(m.VacancyId);
            }

            if (!m.ConfirmDeletion.Value)
            {
                return RedirectToRoute(RouteNames.Vacancy_Preview_Get);
            }

            var result = await _orchestrator.TryDeleteVacancyAsync(m);

            if (!result)
            {
                ModelState.AddModelError(string.Empty, ErrorMessages.VacancyAlreadyDeleted);
                return await GetDeleteVacancyConfirmationView(m.VacancyId);
            }
            
            return RedirectToRoute(RouteNames.Dashboard_Index_Get);
        }

        private async Task<IActionResult> GetDeleteVacancyConfirmationView(Guid vacancyId)
        {
            var vm = await _orchestrator.GetDeleteViewModelAsync(vacancyId);
            return View(vm);
        }
    }
}
