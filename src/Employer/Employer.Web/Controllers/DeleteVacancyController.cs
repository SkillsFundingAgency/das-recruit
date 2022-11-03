using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.DeleteVacancy;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
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
                    return RedirectToRoute(RouteNames.EmployerTaskListGet, new {m.VacancyId, m.EmployerAccountId});
                }
                return RedirectToRoute(RouteNames.Vacancies_Get, new {m.VacancyId, m.EmployerAccountId});
            }

            var vm = await _orchestrator.DeleteVacancyAsync(m, User.ToVacancyUser());
            TempData.Add(TempDataKeys.DashboardInfoMessage, string.Format(InfoMessages.AdvertDeleted, vm.VacancyReference, vm.Title));
            
            return RedirectToRoute(RouteNames.Vacancies_Get, new {m.EmployerAccountId});
        }

        private async Task<IActionResult> GetDeleteVacancyConfirmationView(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetDeleteViewModelAsync(vrm);
            return View(vm);
        }
    }
}
