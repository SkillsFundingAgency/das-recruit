using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Provider.Web.Controllers;

public class LogoutController(ProviderApprenticeshipsLinkHelper externalLinksOptions, IConfiguration configuration)
    : Controller
{
    [AllowAnonymous]
    [Route("signout", Name = RouteNames.ProviderSignOut)]
    [Route("service/signout", Name = RouteNames.ProviderTimeoutSignOut)]
    public async Task Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
        {
            RedirectUri = externalLinksOptions.AccountHome,
        });
    }

    [HttpGet]
    [Route("dashboard", Name = RouteNames.PasDashboard_Get)]
    public IActionResult Dashboard()
    {
        return RedirectPermanent(configuration["ProviderSharedUIConfiguration:DashboardUrl"]);
    }
}