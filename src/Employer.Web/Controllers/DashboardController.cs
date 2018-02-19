using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Esfa.Recruit.Employer.Web.Extensions;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route("accounts/{employerAccountId}")]
    public class DashboardController : Controller
    {
        ILogger<DashboardController> _logger;
        private readonly IOptions<AuthenticationConfiguration> _authConfig;
        private readonly IOptions<ExternalLinksConfiguration> _extLinksConfig;

        public DashboardController(ILogger<DashboardController> logger, IOptions<AuthenticationConfiguration> authConfig, IOptions<ExternalLinksConfiguration> extLinksConfig)
        {
            _logger = logger;
            _authConfig = authConfig;
            _extLinksConfig = extLinksConfig;
        }
        
        [HttpGet, Route("dashboard", Name = RouteNames.Dashboard_Index_Get)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("change-password", Name = RouteNames.Dashboard_ChangePassword)]
        public IActionResult ChangePassword(string returnUrl)
        {
            var url = string.Format(_extLinksConfig.Value.ManageApprenticeshipSiteChangePasswordUrl, _authConfig.Value.ClientId, $"{Request.GetRequestUrl()}{returnUrl}");
            return Redirect(url);
        }

        [HttpGet("change-email", Name = RouteNames.Dashboard_ChangeEmail)]
        public IActionResult ChangeEmailAddress(string returnUrl)
        {
            var url = string.Format(_extLinksConfig.Value.ManageApprenticeshipSiteChangeEmailAddressUrl, _authConfig.Value.ClientId, $"{Request.GetRequestUrl()}{returnUrl}");
            return Redirect(url);
        }

        [HttpGet("finance", Name = RouteNames.Dashboard_AccountsFinance)]
        public IActionResult AccountsFinance(string employerAccountId)
        {
            var url = string.Format(_extLinksConfig.Value.ManageApprenticeshipSiteAccountsFinanceLink, employerAccountId);
            return Redirect(url);
        }

        [HttpGet("apprentices", Name = RouteNames.Dashboard_AccountsApprentices)]
        public IActionResult AccountsApprentices(string employerAccountId)
        {
            var url = string.Format(_extLinksConfig.Value.ManageApprenticeshipSiteAccountsFinanceLink, employerAccountId);
            return Redirect(url);
        }

        [HttpGet("teams", Name = RouteNames.Dashboard_AccountsTeams)]
        public IActionResult AccountsTeams(string employerAccountId)
        {
            var url = string.Format(_extLinksConfig.Value.ManageApprenticeshipSiteAccountsTeamsViewLink, employerAccountId);
            return Redirect(url);
        }

        [HttpGet("agreements", Name = RouteNames.Dashboard_AccountsAgreements)]
        public IActionResult AccountsAgreements(string employerAccountId)
        {
            var url = string.Format(_extLinksConfig.Value.ManageApprenticeshipSiteAccountsAgreementsLink, employerAccountId);
            return Redirect(url);
        }

        [HttpGet("schemes", Name = RouteNames.Dashboard_AccountsSchemes)]
        public IActionResult AccountsSchemes(string employerAccountId)
        {
            var url = string.Format(_extLinksConfig.Value.ManageApprenticeshipSiteAccountsSchemesLink, employerAccountId);
            return Redirect(url);
        }
    }
}