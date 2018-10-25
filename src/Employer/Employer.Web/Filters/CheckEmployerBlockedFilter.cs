using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Caching;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers;
using Esfa.Recruit.Employer.Web.Exceptions;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Filters
{
    public class CheckEmployerBlockedFilter : IAsyncActionFilter
    {
        private readonly IBlockedEmployersProvider _blockedEmployersProvider;
        private readonly ICache _cache;
        private readonly IEmployerVacancyClient _vacancyClient;
        private readonly IDataProtector _dataProtector;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger<CheckEmployerBlockedFilter> _logger;

        private DateTime CacheAbsoluteExpiryTime => DateTime.UtcNow.AddHours(1);

        public CheckEmployerBlockedFilter(
            IBlockedEmployersProvider blockedEmployersProvider, 
            ICache cache,
            IEmployerVacancyClient vacancyClient,
            IDataProtectionProvider dataProtectionProvider,
            IHostingEnvironment hostingEnvironment,
            ILogger<CheckEmployerBlockedFilter> logger)
        {
            _blockedEmployersProvider = blockedEmployersProvider;
            _cache = cache;
            _vacancyClient = vacancyClient;
            _dataProtector = dataProtectionProvider.CreateProtector("levy-declaration"); //TODO: LWA - Change the purpose
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var blockedEmployerAccountIds = await _cache.CacheAsideAsync(
                CacheKeys.BlockedEmployersCacheKey,
                CacheAbsoluteExpiryTime,
                () => _blockedEmployersProvider.GetBlockedEmployerAccountIdsAsync());

            var accountIdFromUrl = context.RouteData.Values[RouteValues.EmployerAccountId].ToString().ToUpper();

            if (blockedEmployerAccountIds.Contains(accountIdFromUrl))
            {
                throw new BlockedEmployerException($"Employer account '{accountIdFromUrl}' is blocked");
            }


            // TODO: LWA - Move into it's own filter


            if (RequestNotForWhiteListedPage(context) && DoesNotHaveValidLevyCookie(context))
            {
                var userId = context.HttpContext.User.GetUserId();
                var details = await _vacancyClient.GetUsersDetailsAsync(userId);

                if (details.DeclaredAsLevyPayer)
                {
                    var protectedUserId = _dataProtector.Protect(userId);
                    context.HttpContext.Response.Cookies.Append(CookieNames.LevyEmployerIndicator, protectedUserId, EsfaCookieOptions.GetDefaultHttpCookieOption(_hostingEnvironment));
                }
                else
                {
                    var employerAccountId = context.RouteData.Values[RouteValues.EmployerAccountId].ToString().ToUpper();
                    // Redirect to Levy page
                    context.Result = new RedirectToRouteResult(RouteNames.LevyDeclaration_Get, new { employerAccountId = employerAccountId });
                    return;
                }
            }

            await next();
        }

        private bool RequestNotForWhiteListedPage(ActionExecutingContext context)
        {
            var controllerName = (((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)context.ActionDescriptor).ControllerTypeInfo).Name;

            var whitelistControllers = new List<string>{ nameof(LevyDeclarationController), nameof(ErrorController) };
            
            return !whitelistControllers.Contains(controllerName);
        }

        private bool DoesNotHaveValidLevyCookie(ActionExecutingContext context)
        {
            var cookie = context.HttpContext.Request.Cookies[CookieNames.LevyEmployerIndicator];

            if (!string.IsNullOrWhiteSpace(cookie))
            {
                var userId = _dataProtector.Unprotect(cookie);

                var userIdDoesNotMatch = userId != context.HttpContext.User.GetUserId();

                if (userIdDoesNotMatch)
                {
                    _logger.LogWarning($"Current user doesn't match user in Levy Cookie: Current: {userId}, Cookie: {context.HttpContext.User.GetUserId()}");
                    // Delete cookie if it's not for current user.
                    context.HttpContext.Response.Cookies.Delete(CookieNames.LevyEmployerIndicator);
                }

                return userIdDoesNotMatch;
            }
                
            return true;
        }
    }

}
