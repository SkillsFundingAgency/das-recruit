using System;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.Services)]
    public class LogoutController : Controller
    {
        private readonly IConfiguration _configuration;

        public LogoutController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet, Route("logout", Name = RouteNames.Logout_Get)]
        public async Task<IActionResult> Logout()
        {
            if (_configuration["RecruitConfiguration:UseGovSignIn"] != null && _configuration["RecruitConfiguration:UseGovSignIn"]
                    .Equals("true", StringComparison.CurrentCultureIgnoreCase))
            {
                var idToken = await HttpContext.GetTokenAsync("id_token");

                var authenticationProperties = new AuthenticationProperties();
                authenticationProperties.Parameters.Clear();
                authenticationProperties.Parameters.Add("id_token",idToken);
                return SignOut(
                    authenticationProperties, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
            }
            
            
            await HttpContext.SignOutEmployerWebAsync();
            return SignOut();
        }
    }
}