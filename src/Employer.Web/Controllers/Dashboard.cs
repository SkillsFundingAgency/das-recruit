using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route("/{accountId}/dashboard")]
    public class DashboardController : Controller
    {
        ILogger<DashboardController> _logger;

        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string accountId)
        {
            return View();
        }
    }
}