using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Reports
{
    [Route(RoutePaths.ReportsRoutePath)]
    public class ReportDashboardController : Controller
    {
        [HttpGet("dashboard", Name = RouteNames.ReportDashboard_Get)]
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}