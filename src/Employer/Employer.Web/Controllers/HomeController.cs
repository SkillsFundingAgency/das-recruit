using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ExternalLinksConfiguration _externalLinks;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IOptions<ExternalLinksConfiguration> externalLinksOptions, ILogger<HomeController> logger)
        {
            _externalLinks = externalLinksOptions.Value;
            _logger = logger;
        }

        [HttpGet, Route("accounts/{employerAccountId}/home", Name = RouteNames.Home_Index_Get)]
        public IActionResult Index()
        {
            _logger.LogInformation("Showing Index page.");
            return View();
        }

        [HttpGet, Route("accounts/{employerAccountId}/home/logout", Name = RouteNames.Home_Logout_Get)]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync("oidc");

            return Redirect(_externalLinks.ManageApprenticeshipSiteUrl);
        }
    }
}
