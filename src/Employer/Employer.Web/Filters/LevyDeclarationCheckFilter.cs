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
        private readonly IEmployerVacancyClient _vacancyClient;
        private readonly ILogger<LevyDeclarationCheckFilter> _logger;
        private readonly LevyDeclarationCookieWriter _levyCookieWriter;

        public LevyDeclarationCheckFilter(IEmployerVacancyClient vacancyClient,
            ILogger<LevyDeclarationCheckFilter> logger,
            LevyDeclarationCookieWriter levyCookieWriter)
        {
            _vacancyClient = vacancyClient;
            _logger = logger;
            _levyCookieWriter = levyCookieWriter;
        }

        public int Order { get; } = 50;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var employerAccountId = context.RouteData.Values[RouteValues.EmployerAccountId]?.ToString().ToUpper();
            var userId = context.HttpContext.User.GetUserId();
            
            var hasValidCookie = HasValidLevyCookie(context, employerAccountId);
            var levyControllerRequested = RequestIsForALevyPage(context);
            var whiteListedControllerRequested = RequestIsForWhiteListedPage(context);

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
                if (levyControllerRequested || !whiteListedControllerRequested)
                {
                    _levyCookieWriter.WriteCookie(context.HttpContext.Response, userId, employerAccountId);

                    if (levyControllerRequested)
                    {
                        context.Result = new RedirectToRouteResult(RouteNames.Dashboard_Index_Get, new { employerAccountId });
                        return;
                    }
                }

                await next(); 
            }
            else
            {
                if (!levyControllerRequested && !whiteListedControllerRequested)
                {
                    context.Result = new RedirectToRouteResult(RouteNames.LevyDeclaration_Get, new { employerAccountId });
                    return;
                }

                await next(); 
            }
        }

        private async Task<bool> HasStoredDeclaration(string employerAccountId, string userId)
        {
            var details = await _vacancyClient.GetUsersDetailsAsync(userId);

            return details.AccountsDeclaredAsLevyPayers.Contains(employerAccountId);
        }

        private bool RequestIsForWhiteListedPage(ActionExecutingContext context)
        {
            var controllerName = (((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)context.ActionDescriptor).ControllerTypeInfo).Name;

            var whitelistControllers = new List<string>{ nameof(LevyDeclarationController), nameof(ErrorController) };
            
            return whitelistControllers.Contains(controllerName);
        }

        private bool RequestIsForALevyPage(ActionExecutingContext context)
        {
            var controllerName = (((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)context.ActionDescriptor).ControllerTypeInfo).Name;

            return controllerName ==  nameof(LevyDeclarationController);
        }

        private bool HasValidLevyCookie(ActionExecutingContext context, string employerAccountId)
        {
            var cookieUserAccountValue = _levyCookieWriter.GetCookieFromRequest(context.HttpContext.Request);

            if (!string.IsNullOrWhiteSpace(cookieUserAccountValue))
            {
                var currentUserAccountValue = $"{context.HttpContext.User.GetUserId()}-{employerAccountId}";
                var valuesMatch = cookieUserAccountValue == currentUserAccountValue ;

                if (!valuesMatch)
                {
                    _logger.LogWarning($"Current user doesn't match user in Levy Cookie: Current: {currentUserAccountValue}, Cookie: {cookieUserAccountValue}");
                    
                    // Delete cookie if it's not for current user.
                    _levyCookieWriter.DeleteCookie(context.HttpContext.Response);
                }

                return valuesMatch;
            }
                
            return false;
        }
    }
}