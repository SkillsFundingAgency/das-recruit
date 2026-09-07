using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.ShortDescription;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class ShortDescriptionController(ShortDescriptionOrchestrator orchestrator) : Controller
    {
        [HttpGet("description", Name = RouteNames.ShortDescription_Get)]
        public async Task<IActionResult> ShortDescription(VacancyRouteModel vrm)
        {
            var vm = await orchestrator.GetShortDescriptionViewModelAsync(vrm);
            return View(vm);
        }

        [HttpPost("description", Name = RouteNames.ShortDescription_Post)]
        public async Task<IActionResult> ShortDescription(ShortDescriptionEditModel m)
        {
            var response = await orchestrator.PostShortDescriptionEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }
            
            var vm = await orchestrator.GetShortDescriptionViewModelAsync(m);
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            return RedirectToRoute(vm.IsTaskListCompleted 
                ? RouteNames.EmployerCheckYourAnswersGet 
                : RouteNames.VacancyWorkDescription_Index_Get, 
                new {m.VacancyId, m.EmployerAccountId});
        }
    }
}