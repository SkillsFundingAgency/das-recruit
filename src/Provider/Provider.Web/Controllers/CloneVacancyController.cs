using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.ViewModels.CloneVacancy;
using Esfa.Recruit.Provider.Web.Extensions;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class CloneVacancyController : Controller
    {
        private readonly CloneVacancyOrchestrator _orchestrator;
        public CloneVacancyController(CloneVacancyOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("clone", Name = RouteNames.CloneVacancy_Get)]
        public async Task<IActionResult> Clone(VacancyRouteModel vrm)
        {
            return await GetCloneUseDatesConfirmationView(vrm);
        }

        [HttpPost("clone", Name = RouteNames.CloneVacancy_Post)]
        public async Task<IActionResult> Clone(CloneVacancyEditModel model)
        {
            if (!ModelState.IsValid)
            {
                return await GetCloneUseDatesConfirmationView(model);
            }

            if (model.ConfirmClose.GetValueOrDefault())
            {
                var newVacancyId = await _orchestrator.CloneVacancy(model, User.ToVacancyUser());
                return RedirectToRoute(RouteNames.Vacancy_Preview_Get, new { VacancyId = newVacancyId });
            }
            return null;
        }

        private async Task<IActionResult> GetCloneUseDatesConfirmationView(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetCloneVacancyViewModelAsync(vrm);
            return View(ViewNames.CloneVacancyView, vm);
        }
    }
}