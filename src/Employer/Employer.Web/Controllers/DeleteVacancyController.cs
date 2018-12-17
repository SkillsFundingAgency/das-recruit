using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.DeleteVacancy;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class DeleteVacancyController : Controller
    {
        private readonly DeleteVacancyOrchestrator _orchestrator;

        public DeleteVacancyController(DeleteVacancyOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("delete", Name = RouteNames.DeleteVacancy_Delete_Get)]
        public Task<IActionResult> Delete(VacancyRouteModel vrm)
        {
            return GetDeleteVacancyConfirmationView(vrm);
        }

        [HttpPost("delete", Name = RouteNames.DeleteVacancy_Delete_Post)]
        public async Task<IActionResult> Delete(DeleteEditModel m)
        {
            var vrm = new VacancyRouteModel { VacancyId = m.VacancyId, EmployerAccountId = m.EmployerAccountId };

            if (!ModelState.IsValid)
            {
                return await GetDeleteVacancyConfirmationView(vrm);
            }

            if (!m.ConfirmDeletion.Value)
            {
                return RedirectToRoute(RouteNames.Vacancy_Preview_Get);
            }

            await _orchestrator.DeleteVacancyAsync(m, User.ToVacancyUser());
            
            return RedirectToRoute(RouteNames.Dashboard_Index_Get);
        }

        private async Task<IActionResult> GetDeleteVacancyConfirmationView(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetDeleteViewModelAsync(vrm);
            return View(vm);
        }
    }
}
