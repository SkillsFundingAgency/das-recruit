using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.DeleteVacancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class DeleteVacancyController : Controller
    {
        private readonly DeleteVacancyOrchestrator _orchestrator;

        public DeleteVacancyController(DeleteVacancyOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("delete", Name = RouteNames.DeleteVacancy_Get)]
        public Task<IActionResult> Delete(VacancyRouteModel vrm)
        {
            return GetDeleteVacancyConfirmationView(vrm);
        }

        [HttpPost("delete", Name = RouteNames.DeleteVacancy_Post)]
        public async Task<IActionResult> Delete(DeleteEditModel m)
        {
            if (!ModelState.IsValid)
            {
                return await GetDeleteVacancyConfirmationView(m);
            }

            if (!m.ConfirmDeletion.Value)
            {
                return RedirectToRoute(RouteNames.Vacancy_Preview_Get);
            }

            await _orchestrator.DeleteVacancyAsync(m, User.ToVacancyUser());
            
            return RedirectToRoute(RouteNames.Vacancies_Get);
        }

        private async Task<IActionResult> GetDeleteVacancyConfirmationView(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetDeleteViewModelAsync(vrm);
            return View(vm);
        }
    }
}
