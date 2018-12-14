using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class SubmittedController : Controller
    {
        private readonly SubmittedOrchestrator _orchestrator;

        public SubmittedController(SubmittedOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("submitted", Name = RouteNames.Submitted_Index_Get)]
        public async Task<IActionResult> Confirmation(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetVacancySubmittedConfirmationViewModelAsync(vrm);
            return View(vm);
        }
    }
}