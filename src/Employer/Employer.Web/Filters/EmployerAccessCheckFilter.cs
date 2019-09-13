using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers;
using Esfa.Recruit.Employer.Web.Exceptions;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Filters
{
    /// <summary>
    /// Ensures current employer code in URL has permissions to access the request page
    /// </summary>

    /// <remarks>
    /// Using the employer code in the URL this filter will ensure the employer
    /// meets at least one the following criteria
    /// 1: Request is for a page that doesn't require special access
    /// 2: Has a signed levy declaration
    /// 3: Has an Expression Of Interest
    /// 
    /// Expected results:
    /// A: Employers with a levy declaration cannot go to any levy pages - redirect to dashboard
    /// B: Levy employers with no declaration should be redirected to the levy page
    /// C: Non-levy employers with no EOI should be blocked
    /// </remarks>
    public class EmployerAccessCheckFilter : IAsyncActionFilter, IOrderedFilter
    {
        private readonly ILogger<EmployerAccessCheckFilter> _logger;
        private readonly ILevyDeclarationCookieWriter _levyCookieWriter;
        private readonly IEoiAgreementCookieWriter _eoiCookieWriter;
        private readonly IEmployerAccountTypeCookieWriter _employerAccountTypeCookieWriter;
        private readonly IRecruitVacancyClient _recruitVacancyClient;
        private readonly IEmployerAccountProvider _employerAccountProvider;
        private readonly string[] _allowedAccountTypes = new string[] { "levy", "nonlevy" };

        public EmployerAccessCheckFilter(
            ILogger<EmployerAccessCheckFilter> logger,
            ILevyDeclarationCookieWriter levyCookieWriter,
            IRecruitVacancyClient recruitVacancyClient,
            IEmployerAccountProvider employerAccountProvider,
            IEoiAgreementCookieWriter eoiCookieWriter,
            IEmployerAccountTypeCookieWriter employerAccountTypeCookieWriter)
        {
            _logger = logger;
            _levyCookieWriter = levyCookieWriter;
            _recruitVacancyClient = recruitVacancyClient;
            _employerAccountProvider = employerAccountProvider;
            _eoiCookieWriter = eoiCookieWriter;
            _employerAccountTypeCookieWriter = employerAccountTypeCookieWriter;
        }

        public int Order { get; } = 50;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (RequestIsForWhiteListedPage(context))
            {
                await next();
                return;
            }

            bool requestIsForALevyPage = RequestIsForALevyPage(context);

            var employerAccountId = context.RouteData.Values[RouteValues.EmployerAccountId]?.ToString().ToUpper();
            var userId = context.HttpContext.User.GetUserId();

            if (await WasHandledByEoi(
                context,
                next,
                userId: userId,
                employerAccountId: employerAccountId))
            {
                return;
            }

            if (await WasHandledByLevyDeclaration(
                context,
                next,
                userId: userId,
                employerAccountId: employerAccountId,
                requestIsForALevyPage: requestIsForALevyPage))
            {
                return;
            }

            if (requestIsForALevyPage)
            {
                await next();
                return;
            }

            // No access to the requested page, so show a "Blocked" message for NonLevy
            // or redirect to the Levy Declaration page for levy employers
            string employerAccountType = await GetEmployerAccountType(context, userId, employerAccountId);
            if (string.Compare(employerAccountType, "levy", true) == 0)
            {
                context.Result = new RedirectToRouteResult(RouteNames.LevyDeclaration_Get, new { employerAccountId });
            }
            else
            {
                throw new BlockedEmployerException($"Employer account '{employerAccountId}' is blocked");
            }
        }

        private async Task<string> GetEmployerAccountType(ActionExecutingContext context, string userId, string employerAccountId)
        {
            string result;
            if (!GetCookieValueForUserAndEmployer(context.HttpContext, 
                userId: userId,
                employerAccountId: employerAccountId,
                readValue: _employerAccountTypeCookieWriter.GetCookieFromRequest,
                result: out result))
            {
                EmployerAccountDetails accountDetails = await _employerAccountProvider.GetEmployerAccountDetailsAsync(employerAccountId);
                result = accountDetails.ApprenticeshipEmployerType;

                if (!_allowedAccountTypes.Any(x => string.Compare(accountDetails.ApprenticeshipEmployerType, x, true) == 0))
                    throw new BlockedEmployerException($"Unknown account type {accountDetails.ApprenticeshipEmployerType}");

                _employerAccountTypeCookieWriter.WriteCookie(context.HttpContext.Response, userId, employerAccountId, result);
            }
            return result;
        }

        private async Task<bool> WasHandledByLevyDeclaration(
            ActionExecutingContext context,
            ActionExecutionDelegate next,
            string userId,
            string employerAccountId,
            bool requestIsForALevyPage)
        {
            bool hasLevyDeclaration = false;
            if (GetCookieValueForUserAndEmployer(
                context.HttpContext,
                userId: userId,
                employerAccountId: employerAccountId,
                readValue: _levyCookieWriter.GetCookieFromRequest,
                result: out string hasLevyDeclarationAsString))
            {
                hasLevyDeclaration = bool.Parse(hasLevyDeclarationAsString);
            }
            else
            {
                hasLevyDeclaration = await HasStoredLevyDeclaration(
                    employerAccountId: employerAccountId,
                    userId: userId);
                _levyCookieWriter.WriteCookie(context.HttpContext.Response, userId, employerAccountId, hasLevyDeclaration);
            }

            // No Levy declaration = not handled
            if (!hasLevyDeclaration)
                return false;

            // Don't allow Levy pages if Levy is already declared
            if (requestIsForALevyPage)
            {
                context.Result = new RedirectToRouteResult(RouteNames.Dashboard_Get, new { employerAccountId });
                return true;
            }

            // Allow pages when Levy declaration is present
            await next();
            return true;
        }

        private async Task<bool> WasHandledByEoi(
            ActionExecutingContext context,
            ActionExecutionDelegate next,
            string userId,
            string employerAccountId)
        {
            bool hasEoi = false;
            if (GetCookieValueForUserAndEmployer(
                context.HttpContext,
                userId: userId,
                employerAccountId: employerAccountId,
                readValue: _eoiCookieWriter.GetCookieFromRequest,
                result: out string hasEoiAsString))
            {
                hasEoi = bool.Parse(hasEoiAsString);
            }
            else
            {
                hasEoi = await GetEmployerHasEoi(employerAccountId);
                _eoiCookieWriter.WriteCookie(context.HttpContext.Response, userId, employerAccountId, hasEoi);

                // Why redirect to the dashboard?
                if (hasEoi)
                {
                    context.Result = new RedirectToRouteResult(RouteNames.Dashboard_Get, new { employerAccountId });
                    return true;
                }
            }

            if (hasEoi)
            {
                await next();
                return true;
            }

            return false;
        }

        private async Task<bool> HasStoredLevyDeclaration(string employerAccountId, string userId)
        {
            var details = await _recruitVacancyClient.GetUsersDetailsAsync(userId);

            return details.AccountsDeclaredAsLevyPayers.Contains(employerAccountId);
        }

        private bool RequestIsForWhiteListedPage(ActionExecutingContext context)
        {
            var controllerName = (((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)context.ActionDescriptor).ControllerTypeInfo).Name;

            var whitelistControllers = new List<string> { nameof(ErrorController), nameof(LogoutController), nameof(ExternalLinksController), nameof(ContentPolicyReportController) };

            return whitelistControllers.Contains(controllerName);
        }

        private bool RequestIsForALevyPage(ActionExecutingContext context)
        {
            var controllerName = (((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)context.ActionDescriptor).ControllerTypeInfo).Name;

            return controllerName == nameof(LevyDeclarationController);
        }

        private async Task<bool> GetEmployerHasEoi(string employerAccountId)
        {
            var account = await _employerAccountProvider.GetEmployerAccountDetailsAsync(employerAccountId);
            return account.AccountAgreementType == AccountAgreementType.NonLevyExpressionOfInterest;
        }

        private bool GetCookieValueForUserAndEmployer(
            HttpContext httpContext,
            string userId,
            string employerAccountId,
            Func<HttpContext, string> readValue,
            out string result)
        {
            string cookieValue = readValue(httpContext) ?? "";

            string[] cookieValueParts = cookieValue.Split('/');
            result = null;

            if (cookieValueParts.Length != 3)
                return false;

            string cookieUserIdAndEmployerId = cookieValueParts[0] + "/" + cookieValueParts[1];
            string expectedUserIdAndEmployerId = userId + "/" + employerAccountId;

            if (cookieUserIdAndEmployerId != expectedUserIdAndEmployerId)
                return false;

            result = cookieValueParts[2];
            return true;
        }
    }
}