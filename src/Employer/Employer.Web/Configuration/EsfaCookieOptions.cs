using Esfa.Recruit.Shared.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Esfa.Recruit.Employer.Web.Configuration
{
    public static class EsfaCookieOptions
    {
        public static CookieOptions GetDefaultHttpCookieOption(IHostingEnvironment env) => new CookieOptions
        {
            Secure = !env.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            HttpOnly = true
        };

        public static CookieOptions GetDefaultBrowserCookieOption(IHostingEnvironment env) => new CookieOptions
        {
            Secure = !env.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            HttpOnly = false
        };
    }
}
