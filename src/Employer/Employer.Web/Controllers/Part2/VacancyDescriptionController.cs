using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.VacancyDescription;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class VacancyDescriptionController(VacancyDescriptionOrchestrator orchestrator) : Controller
    {
        [HttpGet("vacancy-description", Name = RouteNames.VacancyDescription_Index_Get)]
        public async Task<IActionResult> VacancyDescription(VacancyRouteModel vrm)
        {
            var vm = await orchestrator.GetVacancyDescriptionViewModelAsync(vrm);
            return View(vm);
        }

        [HttpPost("vacancy-description", Name =  RouteNames.VacancyDescription_Index_Post)]
        public async Task<IActionResult> VacancyDescription(VacancyDescriptionEditModel m)
        {
            var response = await orchestrator.PostVacancyDescriptionEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            var vm = await orchestrator.GetVacancyDescriptionViewModelAsync(m);
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