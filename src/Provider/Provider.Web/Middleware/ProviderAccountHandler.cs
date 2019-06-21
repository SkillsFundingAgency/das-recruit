using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.Providers.Api.Client;

namespace Esfa.Recruit.Provider.Web.Middleware
{
    public class ProviderAccountHandler : AuthorizationHandler<ProviderAccountRequirement>
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IProviderVacancyClient _client;
        private readonly IProviderApiClient _roatpClient;

        private readonly Predicate<Claim> _ukprnClaimFinderPredicate = c => c.Type.Equals(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier);

        public ProviderAccountHandler(IHostingEnvironment hostingEnvironment, IProviderVacancyClient client, IProviderApiClient roatpClient)
        {
            _hostingEnvironment = hostingEnvironment;
            _client = client;
            _roatpClient = roatpClient;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ProviderAccountRequirement requirement)
        {
            var authorized = HasServiceAuthorization(context) &&
                             HasUkprnAuthorization(context) &&
                             HasRoatpAuthorization(context);

            if (authorized)
            {
                if (HasDoneOncePerAuthorizedSessionActions(context) == false)
                {
                    //Run actions that must be done only once per authorized session
                    await SetupProvider(context);

                    SetOncePerAuthorizedSessionActionsCompleted(context);
                }
                
                context.Succeed(requirement);
            }
        }

        private bool HasServiceAuthorization(AuthorizationHandlerContext context)
        {
            Predicate<Claim> serviceClaimFinderPredicate = c => c.Type.Equals(ProviderRecruitClaims.IdamsUserServiceTypeClaimTypeIdentifier);

            if (context.User.HasClaim(serviceClaimFinderPredicate))
            {
                var serviceClaims = context.User.FindAll(serviceClaimFinderPredicate);

                return serviceClaims.Any(claim => claim.Value.Equals(ProviderRecruitClaims.ServiceClaimValue));
            }

            return false;
        }

        private bool HasUkprnAuthorization(AuthorizationHandlerContext context)
        {
            if (context.Resource is AuthorizationFilterContext mvcContext && mvcContext.RouteData.Values.ContainsKey(RouteValues.Ukprn))
            {
                if (context.User.HasClaim(_ukprnClaimFinderPredicate))
                {
                    var ukprnFromClaim = context.User.FindFirst(_ukprnClaimFinderPredicate).Value;
                    var ukprnFromUrl = mvcContext.RouteData.Values[RouteValues.Ukprn].ToString();

                    if (!string.IsNullOrEmpty(ukprnFromUrl) && ukprnFromUrl.Equals(ukprnFromClaim))
                    {
                        mvcContext.HttpContext.Items.Add(ContextItemKeys.ProviderIdentifier, ukprnFromClaim);

                        return true;
                    }
                }
            }
            else
            {
                return true;
            }

            return false;
        }

        private bool HasRoatpAuthorization(AuthorizationHandlerContext context)
        {
            if (HasDoneOncePerAuthorizedSessionActions(context))
                return true;

            try
            {
                var ukprnFromClaim = context.User.FindFirst(_ukprnClaimFinderPredicate).Value;
                var provider = _roatpClient.Get(ukprnFromClaim);
                return provider != null;
            }
            catch (Exception)
            {
                return false;
            } 
        }
        
        private async Task SetupProvider(AuthorizationHandlerContext context)
        {
            if (context.Resource is AuthorizationFilterContext mvcContext && 
                mvcContext.RouteData.Values.ContainsKey(RouteValues.Ukprn))
            {
                var ukprn = context.User.FindFirst(_ukprnClaimFinderPredicate).Value;

                await _client.SetupProviderAsync(long.Parse(ukprn));
            }
        }

        private bool HasDoneOncePerAuthorizedSessionActions(AuthorizationHandlerContext context)
        {
            if (!(context.Resource is AuthorizationFilterContext mvcContext))
                return false;

            var ukprn = context.User.FindFirst(_ukprnClaimFinderPredicate).Value;

            var cookieKey = GetOncePerAuthorizedSessionCookieKey(ukprn);
            var cookie = mvcContext.HttpContext.Request.Cookies[cookieKey];

            return cookie != null;
        }

        private void SetOncePerAuthorizedSessionActionsCompleted(AuthorizationHandlerContext context)
        {
            if (!(context.Resource is AuthorizationFilterContext mvcContext))
                return;

            var ukprn = context.User.FindFirst(_ukprnClaimFinderPredicate).Value;
            var cookieKey = GetOncePerAuthorizedSessionCookieKey(ukprn);

            mvcContext.HttpContext.Response.Cookies.Append(cookieKey, "1", EsfaCookieOptions.GetDefaultHttpCookieOption(_hostingEnvironment));
        }

        private string GetOncePerAuthorizedSessionCookieKey(string ukprn)
        {
            return string.Format(CookieNames.SetupProvider, ukprn);
        }
    }
}