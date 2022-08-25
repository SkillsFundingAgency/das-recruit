using System;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Esfa.Recruit.Employer.Web.Configuration
{
    public static class EsfaCookieOptions
    {
        public static CookieOptions GetDefaultHttpCookieOption(IWebHostEnvironment env) =>
            new CookieOptions
            {
                Secure = !env.IsDevelopment(),
                SameSite = !env.IsDevelopment() ? SameSiteMode.Strict : SameSiteMode.Lax,
                HttpOnly = true
            };

        public static CookieOptions GetSessionLifetimeHttpCookieOption(IWebHostEnvironment env) =>
            GetDefaultHttpCookieOption(env)
            .WithExpiryTime(DateTimeOffset.UtcNow.AddMinutes(AuthenticationConfiguration.SessionTimeoutMinutes));

        public static CookieOptions GetSingleDayLifetimeHttpCookieOption(IWebHostEnvironment env, ITimeProvider timeProvider) =>
            GetDefaultHttpCookieOption(env)
            .WithExpiryTime(timeProvider.NextDay);

        public static CookieOptions GetTenYearHttpCookieOption(IWebHostEnvironment env) =>
            GetDefaultHttpCookieOption(env)
            .WithExpiryTime(DateTimeOffset.UtcNow.AddYears(10));

        public static CookieOptions WithExpiryTime(this CookieOptions cookieOptions, DateTimeOffset expires)
        {
            cookieOptions.Expires = expires;
            return cookieOptions;
        }
    }
}