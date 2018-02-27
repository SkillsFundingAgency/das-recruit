﻿using Esfa.Recruit.Employer.Web.Configuration;
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
        private readonly ManageApprenticeshipsLinkHelper _linkHelper;

        public ExternalLinksController(ILogger<ExternalLinksController> logger, IOptions<AuthenticationConfiguration> authConfig, ManageApprenticeshipsLinkHelper linkHelper)
        {
            _logger = logger;
            _authConfig = authConfig.Value;
            _linkHelper = linkHelper;
        }

        [HttpGet("change-email", Name = RouteNames.Dashboard_ChangeEmail)]
        public IActionResult ChangeEmailAddress(string returnUrl)
        {
            var encodedReturnUrl = WebUtility.UrlEncode($"{Request.GetRequestUrlRoot()}{returnUrl}");
            var url = string.Format(_linkHelper.ChangeEmail, _authConfig.ClientId, encodedReturnUrl);
            return Redirect(url);
        }

        [HttpGet("change-password", Name = RouteNames.Dashboard_ChangePassword)]
        public IActionResult ChangePassword(string returnUrl)
        {
            var encodedReturnUrl = WebUtility.UrlEncode($"{Request.GetRequestUrlRoot()}{returnUrl}");            
            var url = string.Format(_linkHelper.ChangePassword, _authConfig.ClientId, encodedReturnUrl);
            return Redirect(url);
        }

        [HttpGet("rename-account", Name = RouteNames.Dashboard_AccountsRename)]
        public IActionResult RenameAccount(string employerAccountId)
        {
            var url = string.Format(_linkHelper.RenameAccount, employerAccountId);
            return Redirect(url);
        }

        [HttpGet("finance", Name = RouteNames.Dashboard_AccountsFinance)]
        public IActionResult AccountsFinance(string employerAccountId)
        {
            var url = string.Format(_linkHelper.Finance, employerAccountId);
            return Redirect(url);
        }

        [HttpGet("apprentices", Name = RouteNames.Dashboard_AccountsApprentices)]
        public IActionResult AccountsApprentices(string employerAccountId)
        {
            var url = string.Format(_linkHelper.Apprentices, employerAccountId);
            return Redirect(url);
        }

        [HttpGet("teams", Name = RouteNames.Dashboard_AccountsTeams)]
        public IActionResult AccountsTeams(string employerAccountId)
        {
            var url = string.Format(_linkHelper.Teams, employerAccountId);
            return Redirect(url);
        }

        [HttpGet("agreements", Name = RouteNames.Dashboard_AccountsAgreements)]
        public IActionResult AccountsAgreements(string employerAccountId)
        {
            var url = string.Format(_linkHelper.Agreements, employerAccountId);
            return Redirect(url);
        }

        [HttpGet("schemes", Name = RouteNames.Dashboard_AccountsSchemes)]
        public IActionResult AccountsSchemes(string employerAccountId)
        {
            var url = string.Format(_linkHelper.Schemes, employerAccountId);
            return Redirect(url);
        }
    }
}