using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using SFA.DAS.MA.Shared.UI.Configuration;
using SFA.DAS.MA.Shared.UI.Models;

namespace Esfa.Recruit.Provider.Web.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static ICookieBannerViewModel GetCookieBannerViewModel(this IHtmlHelper html)
        {
            var externalLinks = (html.ViewContext.HttpContext.RequestServices.GetService(typeof(IOptions<ExternalLinksConfiguration>)) as IOptions<ExternalLinksConfiguration>)?.Value;

            return new CookieBannerViewModel(new CookieBannerConfiguration
            {
                ManageApprenticeshipsBaseUrl = externalLinks?.ManageApprenticeshipSiteUrl
            },
                new UserContext
                {
                    User = html.ViewContext.HttpContext.User,
                    HashedAccountId = html.ViewContext.RouteData.Values["employerAccountId"]?.ToString()
                }) ;
        }
    }
}
