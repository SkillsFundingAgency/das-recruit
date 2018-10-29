using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Microsoft.AspNetCore.Hosting;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePrefixPaths.AccountRoutePath)]
    public class LogoutController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ExternalLinksConfiguration _externalLinks;

        public LogoutController(IHostingEnvironment hostingEnvironment, IOptions<ExternalLinksConfiguration> externalLinksOptions)
        {
            _hostingEnvironment = hostingEnvironment;
            _externalLinks = externalLinksOptions.Value;
        }

        [HttpGet, Route("logout", Name = RouteNames.Logout_Get)]
        public async Task Logout()
        {
            await HttpContext.SignOutEmployerWebAsync();
        }
            
    }
}
