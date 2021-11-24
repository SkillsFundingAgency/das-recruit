using System;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using SFA.DAS.MA.Shared.UI.Configuration;
using SFA.DAS.MA.Shared.UI.Models;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static IHeaderViewModel GetHeaderViewModel(this IHtmlHelper html, bool hideMenu = false)
        {
            var externalLinks = (html.ViewContext.HttpContext.RequestServices.GetService(typeof(IOptions<ExternalLinksConfiguration>)) as IOptions<ExternalLinksConfiguration>)?.Value;
            var authConfig = (html.ViewContext.HttpContext.RequestServices.GetService(typeof(IOptions<AuthenticationConfiguration>)) as IOptions<AuthenticationConfiguration>)?.Value;
            var requestRoot = html.ViewContext.HttpContext.Request.GetRequestUrlRoot();
            var requestPath = html.ViewContext.HttpContext.Request.Path;

            var headerModel = new HeaderViewModel(new HeaderConfiguration
                {
                    EmployerCommitmentsV2BaseUrl = externalLinks?.CommitmentsSiteUrl,
                    EmployerFinanceBaseUrl = externalLinks?.ManageApprenticeshipSiteUrl,
                    ManageApprenticeshipsBaseUrl = externalLinks?.ManageApprenticeshipSiteUrl,
                    AuthenticationAuthorityUrl = authConfig?.Authority,
                    ClientId = authConfig?.ClientId,
                    EmployerRecruitBaseUrl = requestRoot,
                    SignOutUrl = new Uri($"{requestRoot}/services/logout/"),
                    ChangeEmailReturnUrl = new Uri($"{requestRoot}{requestPath}"),
                    ChangePasswordReturnUrl = new Uri($"{requestRoot}{requestPath}")
                },
                new UserContext
                {
                    User = html.ViewContext.HttpContext.User,
                    HashedAccountId = html.ViewContext.RouteData.Values["employerAccountId"]?.ToString()
                });

            headerModel.SelectMenu("recruitment");

            if ((html.ViewBag.IsErrorPage is bool && html.ViewBag.IsErrorPage) ||
                (html.ViewBag.ShowNav is bool && !html.ViewBag.ShowNav))
            {
                headerModel.HideMenu();
            }

            return headerModel;
        }

        public static IFooterViewModel GetFooterViewModel(this IHtmlHelper html)
        {
            var externalLinks = (html.ViewContext.HttpContext.RequestServices.GetService(typeof(IOptions<ExternalLinksConfiguration>)) as IOptions<ExternalLinksConfiguration>)?.Value;

            return new FooterViewModel(new FooterConfiguration
                {
                    ManageApprenticeshipsBaseUrl = externalLinks?.ManageApprenticeshipSiteUrl,
                    AuthenticationAuthorityUrl = externalLinks?.EmployerIdamsSiteUrl
                },
                new UserContext
                {
                    User = html.ViewContext.HttpContext.User,
                    HashedAccountId = html.ViewContext.RouteData.Values["employerAccountId"]?.ToString()
                });
        }

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
                });
        }
    }
}
