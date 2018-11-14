using System;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
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

        public static CookieOptions GetSingleDayLifetimeHttpCookieOption(IHostingEnvironment env, ITimeProvider timeProvider) => new CookieOptions
        {
            Secure = !env.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            HttpOnly = true,
            Expires = timeProvider.NextDay
        };
    }
}