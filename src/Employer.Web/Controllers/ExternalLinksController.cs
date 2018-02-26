using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId}")]
    public class ExternalLinksController : Controller
    {
        ILogger<ExternalLinksController> _logger;
        private readonly AuthenticationConfiguration _authConfig;
        private readonly ExternalLinksConfiguration _extLinksConfig;

        public ExternalLinksController(ILogger<ExternalLinksController> logger, IOptions<AuthenticationConfiguration> authConfig, IOptions<ExternalLinksConfiguration> extLinksConfig)
        {
            _logger = logger;
            _authConfig = authConfig.Value;
            _extLinksConfig = extLinksConfig.Value;
        }

        [HttpGet("change-password", Name = RouteNames.Dashboard_ChangePassword)]
        public IActionResult ChangePassword(string returnUrl)
        {
            var encodedReturnUrl = WebUtility.UrlEncode($"{Request.GetRequestUrlRoot()}{returnUrl}");            
            var url = string.Format(_extLinksConfig.ManageApprenticeshipSiteChangePasswordUrl, _authConfig.ClientId, encodedReturnUrl);
            return Redirect(url);
        }

        [HttpGet("change-email", Name = RouteNames.Dashboard_ChangeEmail)]
        public IActionResult ChangeEmailAddress(string returnUrl)
        {
            var encodedReturnUrl = WebUtility.UrlEncode($"{Request.GetRequestUrlRoot()}{returnUrl}");
            var url = string.Format(_extLinksConfig.ManageApprenticeshipSiteChangeEmailAddressUrl, _authConfig.ClientId, encodedReturnUrl);
            return Redirect(url);
        }

        [HttpGet("finance", Name = RouteNames.Dashboard_AccountsFinance)]
        public IActionResult AccountsFinance(string employerAccountId)
        {
            var url = string.Format(_extLinksConfig.ManageApprenticeshipSiteAccountsFinanceLink, employerAccountId);
            return Redirect(url);
        }

        [HttpGet("apprentices", Name = RouteNames.Dashboard_AccountsApprentices)]
        public IActionResult AccountsApprentices(string employerAccountId)
        {
            var url = string.Format(_extLinksConfig.ManageApprenticeshipSiteAccountsFinanceLink, employerAccountId);
            return Redirect(url);
        }

        [HttpGet("teams", Name = RouteNames.Dashboard_AccountsTeams)]
        public IActionResult AccountsTeams(string employerAccountId)
        {
            var url = string.Format(_extLinksConfig.ManageApprenticeshipSiteAccountsTeamsViewLink, employerAccountId);
            return Redirect(url);
        }

        [HttpGet("agreements", Name = RouteNames.Dashboard_AccountsAgreements)]
        public IActionResult AccountsAgreements(string employerAccountId)
        {
            var url = string.Format(_extLinksConfig.ManageApprenticeshipSiteAccountsAgreementsLink, employerAccountId);
            return Redirect(url);
        }

        [HttpGet("schemes", Name = RouteNames.Dashboard_AccountsSchemes)]
        public IActionResult AccountsSchemes(string employerAccountId)
        {
            var url = string.Format(_extLinksConfig.ManageApprenticeshipSiteAccountsSchemesLink, employerAccountId);
            return Redirect(url);
        }
    }
}