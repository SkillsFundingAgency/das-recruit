using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountRoutePath)]
    public class DashboardController : Controller
    {
        private readonly DashboardOrchestrator _orchestrator;
        private readonly IHostingEnvironment _hostingEnvironment;

        public DashboardController(DashboardOrchestrator orchestrator, IHostingEnvironment hostingEnvironment)
        {
            _orchestrator = orchestrator;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet("", Name = RouteNames.Dashboard_Get)]
        public async Task<IActionResult> Dashboard([FromQuery] string filter, [FromQuery] int page = 1)
        {
            if (string.IsNullOrWhiteSpace(filter))
                filter = Request.Cookies.GetCookie(CookieNames.DashboardFilter);

            if (string.IsNullOrWhiteSpace(filter) == false)
                Response.Cookies.SetSessionCookie(_hostingEnvironment, CookieNames.DashboardFilter, filter);

            var vm = await _orchestrator.GetDashboardViewModelAsync(User.GetUkprn());
            return View(vm);
        }
    }
}