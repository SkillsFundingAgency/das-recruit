using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    public class ExternalLinksController : Controller
    {
        private readonly AuthenticationConfiguration _authConfig;
        private readonly ManageApprenticeshipsLinkHelper _linkHelper;

        public ExternalLinksController(IOptions<AuthenticationConfiguration> authConfig, ManageApprenticeshipsLinkHelper linkHelper)
        {
            _authConfig = authConfig.Value;
            _linkHelper = linkHelper;
        }

        [HttpGet(RoutePaths.AccountLevelServices + "/account-home", Name = RouteNames.Dashboard_Account_Home)]
        public IActionResult AccountHome(string employerAccountId)
        {
            var url = string.Format(_linkHelper.AccountHome, employerAccountId);
            return Redirect(url);
        }

        [HttpGet(RoutePaths.Services + "/change-email", Name = RouteNames.Dashboard_ChangeEmail)]
        public IActionResult ChangeEmailAddress(string returnUrl)
        {
            var encodedReturnUrl = WebUtility.UrlEncode($"{Request.GetRequestUrlRoot()}{returnUrl}");
            var url = string.Format(_linkHelper.ChangeEmail, _authConfig.ClientId, encodedReturnUrl);
            return Redirect(url);
        }

        [HttpGet(RoutePaths.Services + "/change-password", Name = RouteNames.Dashboard_ChangePassword)]
        public IActionResult ChangePassword(string returnUrl)
        {
            var encodedReturnUrl = WebUtility.UrlEncode($"{Request.GetRequestUrlRoot()}{returnUrl}");            
            var url = string.Format(_linkHelper.ChangePassword, _authConfig.ClientId, encodedReturnUrl);
            return Redirect(url);
        }

        [HttpGet(RoutePaths.AccountLevelServices + "/rename-account", Name = RouteNames.Dashboard_AccountsRename)]
        public IActionResult RenameAccount(string employerAccountId)
        {
            var url = string.Format(_linkHelper.RenameAccount, employerAccountId);
            return Redirect(url);
        }

        [HttpGet(RoutePaths.AccountLevelServices + "/finance", Name = RouteNames.Dashboard_AccountsFinance)]
        public IActionResult AccountsFinance(string employerAccountId)
        {
            var url = string.Format(_linkHelper.Finance, employerAccountId);
            return Redirect(url);
        }

        [HttpGet(RoutePaths.AccountLevelServices + "/agreements", Name = RouteNames.Dashboard_AccountsAgreements)]
        public IActionResult AccountsAgreements(string employerAccountId)
        {
            var url = string.Format(_linkHelper.Agreements, employerAccountId);
            return Redirect(url);
        }

        [HttpGet(RoutePaths.AccountLevelServices + "/providers", Name = RouteNames.YourTrainingProviderPermission)]
        public IActionResult YourTrainingProviderPermission(string employerAccountId)
        {
            var url = string.Format(_linkHelper.YourTrainingProviderPermission, employerAccountId);
            return Redirect(url);
        }

        [HttpGet(RoutePaths.AccountLevelServices + "/recruitment/api", Name = RouteNames.EmployerRecruitmentApi)]
        public IActionResult EmployerRecruitmentApi(string employerAccountId)
        {
            var url = string.Format(_linkHelper.EmployerRecruitmentApi, employerAccountId);
            return Redirect(url);
        }
    }
}