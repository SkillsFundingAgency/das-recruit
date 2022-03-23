using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.DeleteVacancy;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class DeleteVacancyController : Controller
    {
        private readonly DeleteVacancyOrchestrator _orchestrator;
        private readonly IFeature _feature;

        public DeleteVacancyController(DeleteVacancyOrchestrator orchestrator, IFeature feature)
        {
            _orchestrator = orchestrator;
            _feature = feature;
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
                if (_feature.IsFeatureEnabled(FeatureNames.EmployerTaskList))
                {
                    return RedirectToRoute(RouteNames.VacancyAdvertPreview);
                }
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
