using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Microsoft.AspNetCore.Authorization;
using Esfa.Recruit.Employer.Web.Middleware;
using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasEmployerOwnerOrTransactorAccount))]
    public class VacancyAnalyticsController(IVacancyAnalyticsOrchestrator orchestrator) : Controller
    {
        [HttpGet("analytics", Name = RouteNames.VacancyAnalytics_Get)]
        public async Task<IActionResult> VacancyAnalytics(VacancyRouteModel vrm)
        {
            var viewModel = await orchestrator.GetVacancyAnalytics(vrm);

            return View(viewModel);
        }
    }
}