using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.VacancyDescription;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class VacancyDescriptionController : Controller
    {
        private readonly VacancyDescriptionOrchestrator _orchestrator;
        private readonly IFeature _feature;

        public VacancyDescriptionController(VacancyDescriptionOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("vacancy-description", Name = RouteNames.VacancyDescription_Index_Get)]
        public async Task<IActionResult> VacancyDescription(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetVacancyDescriptionViewModelAsync(vrm);
            return View(vm);
        }

        [HttpPost("vacancy-description", Name =  RouteNames.VacancyDescription_Index_Post)]
        public async Task<IActionResult> VacancyDescription(VacancyDescriptionEditModel m)
        {
            var response = await _orchestrator.PostVacancyDescriptionEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            var vm = await _orchestrator.GetVacancyDescriptionViewModelAsync(m);
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            return vm.IsTaskListCompleted
                ? RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new { m.VacancyId, m.EmployerAccountId })
                : RedirectToRoute(RouteNames.EmployerTaskListGet, new { m.VacancyId, m.EmployerAccountId });
        }
    }
}