using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Filters
{
    public class LevyDeclarationCheckFilter : IAsyncActionFilter, IOrderedFilter
    {
        private readonly ILogger<LevyDeclarationCheckFilter> _logger;
        private readonly LevyDeclarationCookieWriter _levyCookieWriter;
        private readonly EoiAgreementCookieWriter _eoiCookieWriter;
        private readonly IRecruitVacancyClient _recruitVacancyClient;

        public LevyDeclarationCheckFilter(
            ILogger<LevyDeclarationCheckFilter> logger,
            LevyDeclarationCookieWriter levyCookieWriter,
            IRecruitVacancyClient recruitVacancyClient, EoiAgreementCookieWriter eoiCookieWriter)
        {
            _logger = logger;
            _levyCookieWriter = levyCookieWriter;
            _recruitVacancyClient = recruitVacancyClient;
            _eoiCookieWriter = eoiCookieWriter;
        }

        public int Order { get; } = 50;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (RequestIsForWhiteListedPage(context))
            {
                await next();
                return;
            }

            var employerAccountId = context.RouteData.Values[RouteValues.EmployerAccountId]?.ToString().ToUpper();
            var userId = context.HttpContext.User.GetUserId();
            var hasValidEoiCookie = HasValidEoiCookie(context, employerAccountId);
           
            if (hasValidEoiCookie)
            {
                await next();
            }
            else if (await HasEmployerEoi(employerAccountId))
            {
                _eoiCookieWriter.WriteCookie(context.HttpContext.Response, userId, employerAccountId);
                context.Result = new RedirectToRouteResult(RouteNames.Dashboard_Index_Get, new { employerAccountId });
                return;
            }
            
            var hasValidCookie = HasValidLevyCookie(context, employerAccountId);
            var levyControllerRequested = RequestIsForALevyPage(context);

            if (hasValidCookie)
            {
                if (levyControllerRequested)
                {
                    context.Result = new RedirectToRouteResult(RouteNames.Dashboard_Index_Get, new { employerAccountId });
                    return;
                }

                await next();
            }
            else if (await HasStoredDeclaration(employerAccountId, userId))
            {
                if (levyControllerRequested)
                {
                    _levyCookieWriter.WriteCookie(context.HttpContext.Response, userId, employerAccountId);

                    context.Result = new RedirectToRouteResult(RouteNames.Dashboard_Index_Get, new { employerAccountId });
                    return;
                }

                _levyCookieWriter.WriteCookie(context.HttpContext.Response, userId, employerAccountId);

                await next();
            }
            else
            {
                if (!levyControllerRequested)
                {
                    context.Result = new RedirectToRouteResult(RouteNames.LevyDeclaration_Get, new { employerAccountId });
                    return;
                }

                await next();
            }
        }

        private async Task<bool> HasStoredDeclaration(string employerAccountId, string userId)
        {
            var details = await _recruitVacancyClient.GetUsersDetailsAsync(userId);

            return details.AccountsDeclaredAsLevyPayers.Contains(employerAccountId);
        }

        private bool RequestIsForWhiteListedPage(ActionExecutingContext context)
        {
            var controllerName = (((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)context.ActionDescriptor).ControllerTypeInfo).Name;

            var whitelistControllers = new List<string>{ nameof(ErrorController), nameof(LogoutController), nameof(ExternalLinksController), nameof(ContentPolicyReportController) };
            
            return whitelistControllers.Contains(controllerName);
        }

        private bool RequestIsForALevyPage(ActionExecutingContext context)
        {
            var controllerName = (((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)context.ActionDescriptor).ControllerTypeInfo).Name;

            return controllerName ==  nameof(LevyDeclarationController);
        }

        private Task<bool> HasEmployerEoi(string employerAccountId)
        {
            return _recruitVacancyClient.GetEmployerEOIAsync(employerAccountId);
        }

        private bool HasValidLevyCookie(ActionExecutingContext context, string employerAccountId)
        {
            var cookieUserAccountValue = _levyCookieWriter.GetCookieFromRequest(context.HttpContext);

            if (!string.IsNullOrWhiteSpace(cookieUserAccountValue))
            {
                var currentUserAccountValue = $"{context.HttpContext.User.GetUserId()}-{employerAccountId}";
                var valuesMatch = cookieUserAccountValue == currentUserAccountValue ;

                if (!valuesMatch)
                {
                    _logger.LogDebug($"Current user doesn't match user in Levy Cookie: Current: {currentUserAccountValue}, Cookie: {cookieUserAccountValue}");
                    _levyCookieWriter.DeleteCookie(context.HttpContext.Response);
                }
                return valuesMatch;
            }
            return false;
        }

        private bool HasValidEoiCookie(ActionExecutingContext context, string employerAccountId)
        {
            var cookieUserAccountValue = _eoiCookieWriter.GetCookieFromRequest(context.HttpContext);

            if (!string.IsNullOrWhiteSpace(cookieUserAccountValue))
            {
                var currentUserAccountValue = $"{context.HttpContext.User.GetUserId()}-{employerAccountId}";
                var hasMatchingUserAccountValue = cookieUserAccountValue == currentUserAccountValue;

                if (!hasMatchingUserAccountValue)
                {
                    _logger.LogDebug($"Current user doesn't match user in EOI Cookie: Current: {currentUserAccountValue}, Cookie: {cookieUserAccountValue}");
                    _eoiCookieWriter.DeleteCookie(context.HttpContext.Response);
                }
                return hasMatchingUserAccountValue;
            }
            return false;
        }
    }
}