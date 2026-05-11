using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.Orchestrators;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class VacancyAnalyticsController(IVacancyAnalyticsOrchestrator orchestrator) : Controller
    {
        [HttpGet("analytics", Name = RouteNames.VacancyAnalytics_Get)]
        public async Task<IActionResult> VacancyAnalytics(VacancyRouteModel routeModel)
        {
            var vm = await orchestrator.GetVacancyAnalytics(routeModel);

            return View(vm);
        }
    }
}