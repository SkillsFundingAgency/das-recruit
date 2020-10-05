using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    public class ExternalLinksController : Controller
    {
        private readonly ProviderApprenticeshipsLinkHelper _linkHelper;

        public ExternalLinksController(ProviderApprenticeshipsLinkHelper linkHelper)
        {
            _linkHelper = linkHelper;
        }

        [AllowAnonymous]
        [HttpGet("account-home", Name = RouteNames.Dashboard_Account_Home)]
        public IActionResult AccountHome()
        {
            return Redirect(_linkHelper.AccountHome);
        }

        [AllowAnonymous]
        [HttpGet("notification-settings", Name = RouteNames.Dashboard_AccountsNotifications)]
        public IActionResult AccountsNotifications()
        {
            var url = _linkHelper.Notifications;
            return Redirect(url);
        }

        [HttpGet("manage-apprentices", Name = RouteNames.Dashboard_ManageApprentices)]
        public IActionResult AccountsApprentices(long ukprn)
        {
            var url = string.Format(_linkHelper.Apprentices, ukprn);
            return Redirect(url);
        }

        [HttpGet("your-cohorts", Name = RouteNames.Dashboard_YourCohorts)]
        public IActionResult AccountsCohorts(long ukprn)
        {
            var url = string.Format(_linkHelper.YourCohorts, ukprn);
            return Redirect(url);
        }

        [HttpGet("manage-funding", Name = RouteNames.Dashboard_ManageFunding)]
        public IActionResult ManageFunding(long ukprn)
        {
            var url = string.Format(_linkHelper.ManageFunding, ukprn);
            return Redirect(url);
        }

        [HttpGet("agreements", Name = RouteNames.Dashboard_AccountsAgreements)]
        public IActionResult AccountsAgreements(long ukprn)
        {
            var url = string.Format(_linkHelper.Agreements, ukprn);
            return Redirect(url);
        }

        [HttpGet("provider-cookie-settings", Name = RouteNames.ProviderCookieSettings)]
        public IActionResult CookieSettings()
        {
            return Redirect(_linkHelper.CookieSettings);
        }
        [HttpGet("provider-cookie-details", Name = RouteNames.ProviderCookieDetails)]
        public IActionResult CookieDetails()
        {
            return Redirect(_linkHelper.CookieDetails);
        }
        [AllowAnonymous]
        [HttpGet("help", Name = RouteNames.Dashboard_Help)]
        public IActionResult Help()
        {
            return Redirect(_linkHelper.Help);
        }

        [AllowAnonymous]
        [HttpGet("feedback", Name = RouteNames.Dashboard_Feedback)]
        public IActionResult Feedback()
        {
            return Redirect(_linkHelper.Feedback);
        }

        [AllowAnonymous]
        [HttpGet("privacy", Name = RouteNames.Dashboard_Privacy)]
        public IActionResult PrivacyAndCookies()
        {
            return Redirect(_linkHelper.Privacy);
        }

        [AllowAnonymous]
        [HttpGet("terms", Name = RouteNames.Dashboard_TermsAndConditions)]
        public IActionResult TermsAndConditions()
        {
            return Redirect(_linkHelper.TermsAndConditions);
        }
    }
}