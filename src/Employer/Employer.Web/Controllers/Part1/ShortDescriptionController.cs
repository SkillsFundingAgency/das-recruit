using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.ShortDescription;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class ShortDescriptionController : Controller
    {
        private readonly ShortDescriptionOrchestrator _orchestrator;

        public ShortDescriptionController(ShortDescriptionOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }
        
        [HttpGet("description", Name = RouteNames.ShortDescription_Get)]
        public async Task<IActionResult> ShortDescription(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetShortDescriptionViewModelAsync(vrm);
            return View(vm);
        }

        [HttpPost("description", Name = RouteNames.ShortDescription_Post)]
        public async Task<IActionResult> ShortDescription(ShortDescriptionEditModel m)
        {
            var response = await _orchestrator.PostShortDescriptionEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetShortDescriptionViewModelAsync(m);

                return View(vm);
            }

            return RedirectToRoute(RouteNames.Training_Get);
        }
    }
}