using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    public class LogoutController : Controller
    {
        private readonly ExternalLinksConfiguration _externalLinks;

        public LogoutController(IOptions<ExternalLinksConfiguration> externalLinksOptions)
        {
            _externalLinks = externalLinksOptions.Value;
        }

        [AllowAnonymous]
        [Route("signout", Name = RouteNames.ProviderSignOut)]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(WsFederationDefaults.AuthenticationScheme, new AuthenticationProperties()
            {
                RedirectUri = _externalLinks.ProviderApprenticeshipSiteUrl // TODO: LWA - Need to test if this works!!??
            });
        }
    }
}