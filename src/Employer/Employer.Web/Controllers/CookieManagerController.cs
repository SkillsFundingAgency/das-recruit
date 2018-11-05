using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePrefixPaths.AccountRoutePath)]
    public class CookieManagerController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public CookieManagerController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet("dismiss-outage-message", Name = RouteNames.DismissOutageMessage_Get)]
        public IActionResult DismissOutageMessage()
        {
            const string SeenCookieValue = "1";
            var redirectUrl = Request.Headers["Referer"];

            if (!Request.Cookies.ContainsKey(CookieNames.SeenOutageMessage))
                Response.Cookies.Append(CookieNames.SeenOutageMessage, SeenCookieValue, EsfaCookieOptions.GetSingleDayLifetimeHttpCookieOption(_hostingEnvironment));

            if (!string.IsNullOrEmpty(redirectUrl))
                return Redirect(redirectUrl);

            return RedirectToRoute(RouteNames.Dashboard_Index_Get);
        }
    }
}