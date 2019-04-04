﻿using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Provider.Web.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Esfa.Recruit.Provider.Web.Extensions
{
    public static class CookiesExtensions
    {
        public static void SetSessionCookie(this IResponseCookies cookies, IHostingEnvironment hostingEnvironment, string key, string value)
        {
            cookies.Append(key, value, EsfaCookieOptions.GetSessionLifetimeHttpCookieOption(hostingEnvironment));
        }

        public static void DeleteSessionCookie(this IResponseCookies cookies, IHostingEnvironment hostingEnvironment, string key)
        {
            cookies.Delete(key, EsfaCookieOptions.GetSessionLifetimeHttpCookieOption(hostingEnvironment));
        }

        public static string GetCookie(this IRequestCookieCollection cookies, string key)
        {
            return cookies[key]?.Trim();
        }
        
        public static void SetProposedClosingDate(this IResponseCookies cookies, IHostingEnvironment hostingEnvironment, Guid vacancyId, DateTime date)
        {
            cookies.Append(string.Format(CookieNames.VacancyProposedClosingDate, vacancyId), date.ToShortDateString(), EsfaCookieOptions.GetSessionLifetimeHttpCookieOption(hostingEnvironment));
        }

        public static void ClearProposedClosingDate(this IResponseCookies cookies, IHostingEnvironment hostingEnvironment, Guid vacancyId)
        {
            cookies.Delete(string.Format(CookieNames.VacancyProposedClosingDate, vacancyId), EsfaCookieOptions.GetSessionLifetimeHttpCookieOption(hostingEnvironment));
        }

        public static DateTime? GetProposedClosingDate(this IRequestCookieCollection cookies, Guid vacancyId)
        {
            var proposedClosingDateCookieData = cookies[string.Format(CookieNames.VacancyProposedClosingDate, vacancyId)]?.Trim();

            if (DateTime.TryParse(proposedClosingDateCookieData, out var proposedClosingDate))
                return proposedClosingDate;

            return null;
        }

        public static void SetProposedStartDate(this IResponseCookies cookies, IHostingEnvironment hostingEnvironment, Guid vacancyId, DateTime date)
        {
            cookies.Append(string.Format(CookieNames.VacancyProposedStartDate, vacancyId), date.ToShortDateString(), EsfaCookieOptions.GetSessionLifetimeHttpCookieOption(hostingEnvironment));
        }

        public static void ClearProposedStartDate(this IResponseCookies cookies, IHostingEnvironment hostingEnvironment, Guid vacancyId)
        {
            cookies.Delete(string.Format(CookieNames.VacancyProposedStartDate, vacancyId), EsfaCookieOptions.GetSessionLifetimeHttpCookieOption(hostingEnvironment));
        }

        public static DateTime? GetProposedStartDate(this IRequestCookieCollection cookies, Guid vacancyId)
        {
            var proposedStartDateCookieData = cookies[string.Format(CookieNames.VacancyProposedStartDate, vacancyId)]?.Trim();

            if (DateTime.TryParse(proposedStartDateCookieData, out var proposedStartDate))
                return proposedStartDate;

            return null;
        }

        public static string GetDashboardFilter(this IRequestCookieCollection cookies)
        {
            return cookies[CookieNames.DashboardFilter];
        }

        public static void SetDashboardFilter(this IResponseCookies cookies, IHostingEnvironment hostingEnvironment, string value)
        {
            cookies.Append(CookieNames.DashboardFilter, value, EsfaCookieOptions.GetSessionLifetimeHttpCookieOption(hostingEnvironment));
        }

    }
}
