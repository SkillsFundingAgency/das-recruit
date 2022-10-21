using System.Text.RegularExpressions;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountRoutePath)]
    public class CookieManagerController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ITimeProvider _timeProvider;

        public CookieManagerController(IWebHostEnvironment hostingEnvironment, ITimeProvider timeProvider)
        {
            _hostingEnvironment = hostingEnvironment;
            _timeProvider = timeProvider;
        }

        [HttpPost("dismiss-outage-message", Name = RouteNames.DismissOutageMessage_Post)]
        public IActionResult DismissOutageMessage([FromForm]string returnUrl) => 
            DismissMessage(
                returnUrl: returnUrl,
                cookieName: CookieNames.SeenOutageMessage,
                cookieOptions: EsfaCookieOptions.GetSingleDayLifetimeHttpCookieOption(_hostingEnvironment, _timeProvider));

        [HttpPost("dismiss-cloning-method-changing-message", Name = RouteNames.DismissCloningMethodChangingMessage_Post)]
        public IActionResult DismissCloningMethodChangingMessage([FromForm]string returnUrl) =>
            DismissMessage(
                returnUrl: returnUrl,
                cookieName: CookieNames.HasSeenCloningMethodIsChangingMessage,
                cookieOptions: EsfaCookieOptions.GetTenYearHttpCookieOption(_hostingEnvironment));

        private IActionResult DismissMessage(string returnUrl, string cookieName, CookieOptions cookieOptions)
        {
            const string SeenCookieValue = "1";

            if (!Request.Cookies.ContainsKey(cookieName))
                Response.Cookies.Append(cookieName, SeenCookieValue, cookieOptions);

            if (IsValidReturnUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToRoute(RouteNames.Vacancies_Get);
        }

        private bool IsValidReturnUrl(string returnUrl)
        {
            return Regex.IsMatch(returnUrl, @"^/accounts/[A-Z0-9]{6}/.*");
        }
    }
}