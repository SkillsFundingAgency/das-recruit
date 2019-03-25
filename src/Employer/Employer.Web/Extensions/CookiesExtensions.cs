using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class CookiesExtensions
    {
        public static void ClearEmployerInfo(this IResponseCookies cookies, IHostingEnvironment hostingEnvironment, Guid vacancyId)
        {
            cookies.Delete(string.Format(CookieNames.EmployerInfo, vacancyId), EsfaCookieOptions.GetSessionLifetimeHttpCookieOption(hostingEnvironment));
        }
        public static void SetEmployerInfo(this IResponseCookies cookies, IHostingEnvironment hostingEnvironment, Guid vacancyId, string employerInfo)
        {
            cookies.Append(string.Format(CookieNames.EmployerInfo, vacancyId), employerInfo, EsfaCookieOptions.GetSessionLifetimeHttpCookieOption(hostingEnvironment));
        }

        public static string GetEmployerInfo(this IRequestCookieCollection cookies, Guid vacancyId)
        {
            return cookies[string.Format(CookieNames.EmployerInfo, vacancyId)]?.Trim();
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

        
    }
}
