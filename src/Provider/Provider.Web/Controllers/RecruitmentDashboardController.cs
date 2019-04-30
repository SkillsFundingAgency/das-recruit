using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.VacanciesRoutePath)]
    public class RecruitmentDashboardController : Controller
    {
        private readonly DashboardOrchestrator _orchestrator;
        private readonly IHostingEnvironment _hostingEnvironment;

        public RecruitmentDashboardController(DashboardOrchestrator orchestrator, IHostingEnvironment hostingEnvironment)
        {
            _orchestrator = orchestrator;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet("", Name = RouteNames.RecruitmentDashboard_Get)]
        public ActionResult Dashboard()
        {
            return View();
        }
    }
}