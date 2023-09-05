using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    public class LogoutController : Controller
    {
        private readonly ExternalLinksConfiguration _externalLinks;
        private readonly IConfiguration _configuration;

        public LogoutController(IOptions<ExternalLinksConfiguration> externalLinksOptions, IConfiguration configuration)
        {
            _configuration = configuration;
            _externalLinks = externalLinksOptions.Value;
        }

        [AllowAnonymous]
        [Route("signout", Name = RouteNames.ProviderSignOut)]
        public async Task Logout()
        {
            bool useDfESignIn = _configuration["UseDfESignIn"] != null 
                                && _configuration["UseDfESignIn"].Equals("true", StringComparison.CurrentCultureIgnoreCase);

            string authScheme = useDfESignIn
                ? OpenIdConnectDefaults.AuthenticationScheme
                : WsFederationDefaults.AuthenticationScheme;

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(authScheme, new AuthenticationProperties()
            {
                RedirectUri = _externalLinks.ProviderApprenticeshipSiteUrl // TODO: LWA - Need to test if this works!!??
            });
        }

        [HttpGet]
        [Route("dashboard", Name = RouteNames.PasDashboard_Get)]
        public IActionResult Dashboard()
        {
            return RedirectPermanent(_configuration["ProviderSharedUIConfiguration:DashboardUrl"]);
        }
    }
}