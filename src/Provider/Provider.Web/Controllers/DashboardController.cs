using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountRoutePath)]
    public class DashboardController(DashboardOrchestrator orchestrator) : Controller
    {
        [HttpGet("", Name = RouteNames.Dashboard_Get)]
        public async Task<IActionResult> Dashboard()
        {
            var vm = await orchestrator.GetDashboardViewModelAsync(User.ToVacancyUser());
            return View(vm.HasEmployerReviewPermission ? ViewNames.DashboardWithReview : ViewNames.DashboardNoReview, vm);
        }
    }
}