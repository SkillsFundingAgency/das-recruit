using System.Text.RegularExpressions;
using Esfa.Recruit.Qa.Web.Configuration;
using Esfa.Recruit.Qa.Web.Configuration.Routing;
using Esfa.Recruit.QA.Web.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.QA.Web.Controllers
{
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
        public IActionResult DismissOutageMessage([FromForm]string returnUrl)
        {
            const string SeenCookieValue = "1";

            if (!Request.Cookies.ContainsKey(CookieNames.SeenOutageMessage))
                Response.Cookies.Append(CookieNames.SeenOutageMessage, SeenCookieValue, EsfaCookieOptions.GetSingleDayLifetimeHttpCookieOption(_hostingEnvironment, _timeProvider));

            return RedirectToRoute(RouteNames.Dashboard_Index_Get);
        }
    }}