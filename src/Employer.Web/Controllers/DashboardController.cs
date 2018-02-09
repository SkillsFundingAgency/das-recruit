using Esfa.Recruit.Employer.Web.Configuration.Routes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    public class DashboardController : Controller
    {
        ILogger<DashboardController> _logger;

        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;
        }

        [HttpGet, Route("accounts/{employerAccountId}/dashboard", Name = RouteNames.Dashboard_Index_Get)]
        public IActionResult Index()
        {
            return View();
        }
    }
}