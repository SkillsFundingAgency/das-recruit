using System;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Esfa.Recruit.Provider.Web.Configuration
{
    public static class EsfaCookieOptions
    {
        public static CookieOptions GetDefaultHttpCookieOption(IWebHostEnvironment env) => new CookieOptions
        {
            Secure = !env.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            HttpOnly = true
        };

        public static CookieOptions GetSessionLifetimeHttpCookieOption(IWebHostEnvironment env) => new CookieOptions
        {
            Secure = !env.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            HttpOnly = true,
            Expires = DateTimeOffset.UtcNow.AddMinutes(AuthenticationConfiguration.SessionTimeoutMinutes)
        };

        public static CookieOptions GetSingleDayLifetimeHttpCookieOption(IWebHostEnvironment env, ITimeProvider timeProvider) => new CookieOptions
        {
            Secure = !env.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            HttpOnly = true,
            Expires = timeProvider.NextDay
        };
    }
}