using System.Text.RegularExpressions;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers;

[Route(RoutePaths.AccountRoutePath)]
public class CookieManagerController(IWebHostEnvironment hostingEnvironment, ITimeProvider timeProvider)
    : Controller
{
    [HttpPost("dismiss-outage-message", Name = RouteNames.DismissOutageMessage_Post)]
    public IActionResult DismissOutageMessage([FromForm]string returnUrl)
    {
        const string SeenCookieValue = "1";

        if (!Request.Cookies.ContainsKey(CookieNames.SeenOutageMessage))
            Response.Cookies.Append(CookieNames.SeenOutageMessage, SeenCookieValue, EsfaCookieOptions.GetSingleDayLifetimeHttpCookieOption(hostingEnvironment, timeProvider));

        if (IsValidReturnUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToRoute(RouteNames.VacanciesGetAll);
    }

    private bool IsValidReturnUrl(string returnUrl)
    {
        return Regex.IsMatch(returnUrl, @"^/[0-9]{8}/recruit/.*");
    }
}