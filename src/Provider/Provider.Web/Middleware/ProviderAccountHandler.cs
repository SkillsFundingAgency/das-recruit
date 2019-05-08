using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Esfa.Recruit.Provider.Web.Middleware
{
    public class ProviderAccountHandler : AuthorizationHandler<ProviderAccountRequirement>
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IProviderVacancyClient _client;

        public ProviderAccountHandler(IHostingEnvironment hostingEnvironment, IProviderVacancyClient client)
        {
            _hostingEnvironment = hostingEnvironment;
            _client = client;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ProviderAccountRequirement requirement)
        {
            var authorized = false;

            if (HasServiceAuthorization(context))
            {
                authorized = await HasUkprnAuthorizationAsync(context);
            }

            if (authorized)
            {
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

        private async Task<bool> HasUkprnAuthorizationAsync(AuthorizationHandlerContext context)
        {
            Predicate<Claim> ukprnClaimFinderPredicate = c => c.Type.Equals(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier);

            if (context.Resource is AuthorizationFilterContext mvcContext && mvcContext.RouteData.Values.ContainsKey(RouteValues.Ukprn))
            {
                if (context.User.HasClaim(ukprnClaimFinderPredicate))
                {
                    var ukprnFromClaim = context.User.FindFirst(ukprnClaimFinderPredicate).Value;
                    var ukprnFromUrl = mvcContext.RouteData.Values[RouteValues.Ukprn].ToString();

                    if (!string.IsNullOrEmpty(ukprnFromUrl) && ukprnFromUrl.Equals(ukprnFromClaim))
                    {
                        mvcContext.HttpContext.Items.Add(ContextItemKeys.ProviderIdentifier, ukprnFromClaim);

                        await EnsureProviderIsSetup(mvcContext.HttpContext, long.Parse(ukprnFromClaim));

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
        
        private async Task EnsureProviderIsSetup(HttpContext context, long ukprn)
        {
            var key = string.Format(CookieNames.SetupProvider, ukprn);

            if (context.Request.Cookies[key] == null)
            {
                await _client.SetupProviderAsync(ukprn);
                context.Response.Cookies.Append(key, "1", EsfaCookieOptions.GetDefaultHttpCookieOption(_hostingEnvironment));
            }
        }
    }
}