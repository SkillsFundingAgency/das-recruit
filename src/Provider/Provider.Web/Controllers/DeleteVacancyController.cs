using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.DeleteVacancy;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
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
            var vrm = new VacancyRouteModel { VacancyId = m.VacancyId, Ukprn = m.Ukprn };

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
