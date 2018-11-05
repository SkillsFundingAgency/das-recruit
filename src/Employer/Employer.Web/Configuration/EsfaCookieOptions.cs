using System;
using Employer.Web.Configuration;
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

        public static CookieOptions GetSessionLifetimeHttpCookieOption(IHostingEnvironment env) => new CookieOptions
        {
            Secure = !env.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            HttpOnly = true,
            Expires = DateTimeOffset.UtcNow.AddMinutes(AuthenticationConfiguration.SessionTimeoutMinutes)
        };

        public static CookieOptions GetSingleDayLifetimeHttpCookieOption(IHostingEnvironment env) => new CookieOptions
        {
            Secure = !env.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            HttpOnly = true,
            Expires = Times.NextDay
        };
    }
}