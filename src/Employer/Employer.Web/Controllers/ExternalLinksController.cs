using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers;

public class ExternalLinksController(ManageApprenticeshipsLinkHelper linkHelper) : Controller
{
    [HttpGet(RoutePaths.AccountLevelServices + "/account-home", Name = RouteNames.Dashboard_Account_Home)]
    public IActionResult AccountHome(string employerAccountId)
    {
        var url = string.Format(linkHelper.AccountHome, employerAccountId);
        return Redirect(url);
    }

    [HttpGet(RoutePaths.AccountLevelServices + "/rename-account", Name = RouteNames.Dashboard_AccountsRename)]
    public IActionResult RenameAccount(string employerAccountId)
    {
        var url = string.Format(linkHelper.RenameAccount, employerAccountId);
        return Redirect(url);
    }

    [HttpGet(RoutePaths.AccountLevelServices + "/finance", Name = RouteNames.Dashboard_AccountsFinance)]
    public IActionResult AccountsFinance(string employerAccountId)
    {
        var url = string.Format(linkHelper.Finance, employerAccountId);
        return Redirect(url);
    }

    [HttpGet(RoutePaths.AccountLevelServices + "/agreements", Name = RouteNames.Dashboard_AccountsAgreements)]
    public IActionResult AccountsAgreements(string employerAccountId)
    {
        var url = string.Format(linkHelper.Agreements, employerAccountId);
        return Redirect(url);
    }

    [HttpGet(RoutePaths.AccountLevelServices + "/providers", Name = RouteNames.YourTrainingProviderPermission)]
    public IActionResult YourTrainingProviderPermission(string employerAccountId)
    {
        var url = string.Format(linkHelper.YourTrainingProviderPermission, employerAccountId);
        return Redirect(url);
    }

    [HttpGet(RoutePaths.AccountLevelServices + "/recruitment/api", Name = RouteNames.EmployerRecruitmentApi)]
    public IActionResult EmployerRecruitmentApi(string employerAccountId)
    {
        var url = string.Format(linkHelper.EmployerRecruitmentApi, employerAccountId);
        return Redirect(url);
    }
}