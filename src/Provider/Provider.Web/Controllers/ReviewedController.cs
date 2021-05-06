using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class ReviewedController : Controller
    {
        private readonly ReviewedOrchestrator _orchestrator;

        public ReviewedController(ReviewedOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("reviewed", Name = RouteNames.Reviewed_Index_Get)]
        public async Task<IActionResult> Confirmation(VacancyRouteModel vrm)
        
        {
            var vm = await _orchestrator.GetVacancyReviewedOrchestratorConfirmationViewModelAsync(vrm, User.ToVacancyUser());
            return View(vm);
        }
    }
}