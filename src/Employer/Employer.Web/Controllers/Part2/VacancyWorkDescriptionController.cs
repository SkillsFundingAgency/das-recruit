using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.VacancyWorkDescription;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class VacancyWorkDescriptionController(VacancyWorkDescriptionOrchestrator orchestrator) : Controller
    {
        [HttpGet("vacancy-work-description", Name = RouteNames.VacancyWorkDescription_Index_Get)]
        public async Task<IActionResult> VacancyWorkDescription(VacancyRouteModel vrm)
        {
            var vm = await orchestrator.GetVacancyDescriptionViewModelAsync(vrm);
            return View(vm);
        }

        [HttpPost("vacancy-work-description", Name =  RouteNames.VacancyWorkDescription_Index_Post)]
        public async Task<IActionResult> VacancyWorkDescription(VacancyWorkDescriptionEditModel m)
        {
            var response = await orchestrator.PostVacancyDescriptionEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            var vm = await orchestrator.GetVacancyDescriptionViewModelAsync(m);
            if (!ModelState.IsValid)
            {
                vm.VacancyDescription = m.VacancyDescription;
                return View(vm);
            }
            
            if (vm.IsTaskListCompleted)
            {
                return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new {m.VacancyId, m.EmployerAccountId});
            }
            return RedirectToRoute(RouteNames.VacancyHowTheApprenticeWillTrain_Index_Get, new {m.VacancyId, m.EmployerAccountId});
        }
    }
}