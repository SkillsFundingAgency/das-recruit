using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.DeleteVacancy;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
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
                if (m.Status == VacancyStatus.Draft)
                {
                    return RedirectToRoute(RouteNames.ProviderTaskListGet, new { m.Ukprn, m.VacancyId });
                }
                return RedirectToRoute(RouteNames.Vacancies_Get, new { m.Ukprn });
            }

            var vm = await _orchestrator.DeleteVacancyAsync(m, User.ToVacancyUser());

            TempData.Add(TempDataKeys.VacanciesInfoMessage, string.Format(InfoMessages.VacancyDeleted, vm.VacancyReference, vm.Title));

            return RedirectToRoute(RouteNames.Vacancies_Get, new { m.Ukprn });
        }

        private async Task<IActionResult> GetDeleteVacancyConfirmationView(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetDeleteViewModelAsync(vrm);
            return View(vm);
        }
    }
}
