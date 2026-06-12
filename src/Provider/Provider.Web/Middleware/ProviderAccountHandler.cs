using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Esfa.Recruit.Provider.Web.Middleware
{
    public class ProviderAccountHandler(
        IWebHostEnvironment hostingEnvironment,
        ITempDataProvider tempDataProvider,
        ITrainingProviderSummaryProvider trainingProviderSummaryProvider)
        : AuthorizationHandler<ProviderAccountRequirement>
    {
        private readonly Predicate<Claim> _ukprnClaimFinderPredicate = c => c.Type.Equals(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier) 
                                                                            || c.Type.Equals(ProviderRecruitClaims.DfEUkprnClaimsTypeIdentifier);
        private readonly Dictionary<string, object> _dict = new();

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ProviderAccountRequirement requirement)
        {
            var hasIdentityServerAuthorization = HasServiceAuthorization(context) && HasUkprnAuthorization(context);

            if (context.User.HasClaim(_ukprnClaimFinderPredicate))
            {
                var ukprnFromClaim = context.User.FindFirst(_ukprnClaimFinderPredicate).Value;

                var isOnRoatp = await HasRoatpAuthorizationAsync(context, ukprnFromClaim);

                if (hasIdentityServerAuthorization && isOnRoatp)
                {
                    if (!HasDoneOncePerAuthorizedSessionActions(context))
                    {
                        SetOncePerAuthorizedSessionActionsCompleted(context);
                    }

                    if (context.HasFailed)
                    {
                        var mvcContext = (AuthorizationFilterContext)context.Resource;
                        tempDataProvider.SaveTempData(mvcContext.HttpContext, _dict);
                    }
                    else
                    {
                        context.Succeed(requirement);
                    }
                }
            }
        }

        private static bool HasServiceAuthorization(AuthorizationHandlerContext context)
        {
            Predicate<Claim> serviceClaimFinderPredicate = c => c.Type.Equals(ProviderRecruitClaims.IdamsUserServiceTypeClaimTypeIdentifier) 
                                                                || c.Type.Equals(ProviderRecruitClaims.DfEUserServiceTypeClaimTypeIdentifier);

            if (context.User.HasClaim(serviceClaimFinderPredicate))
            {
                var serviceClaims = context.User.FindAll(serviceClaimFinderPredicate);

                return serviceClaims.Any(claim => claim.Value.IsServiceClaim());
            }

            return false;
        }

        private bool HasUkprnAuthorization(AuthorizationHandlerContext context)
        {
            if (context.Resource is AuthorizationFilterContext mvcContext && mvcContext.RouteData.Values.TryGetValue(RouteValues.Ukprn, out var value))
            {
                if (context.User.HasClaim(_ukprnClaimFinderPredicate))
                {
                    var ukprnFromClaim = context.User.FindFirst(_ukprnClaimFinderPredicate).Value;
                    var ukprnFromUrl = value.ToString();

                    if (!string.IsNullOrEmpty(ukprnFromUrl) && ukprnFromUrl.Equals(ukprnFromClaim))
                    {
                        mvcContext.HttpContext.Items.TryAdd(ContextItemKeys.ProviderIdentifier, ukprnFromClaim);
                        _dict.Add(TempDataKeys.ProviderIdentifier, ukprnFromClaim);

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

        private async Task<bool> HasRoatpAuthorizationAsync(AuthorizationHandlerContext context, string ukprnFromClaim)
        {
            if (HasDoneOncePerAuthorizedSessionActions(context))
                return true;

            try
            {
                if (!long.TryParse(ukprnFromClaim, out var ukprn))
                    return false;

                if (ukprn == EsfaTestTrainingProvider.Ukprn)
                    return true;

                var provider = await trainingProviderSummaryProvider.GetAsync(ukprn);
                
                _dict.Add(TempDataKeys.ProviderName, provider.ProviderName);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool HasDoneOncePerAuthorizedSessionActions(AuthorizationHandlerContext context)
        {
            if (context.Resource is not AuthorizationFilterContext mvcContext)
                return false;

            var ukprn = context.User.FindFirst(_ukprnClaimFinderPredicate).Value;

            var cookieKey = GetOncePerAuthorizedSessionCookieKey(ukprn);
            var cookie = mvcContext.HttpContext.Request.Cookies[cookieKey];

            return cookie != null;
        }

        private void SetOncePerAuthorizedSessionActionsCompleted(AuthorizationHandlerContext context)
        {
            if (context.Resource is not AuthorizationFilterContext mvcContext)
                return;

            var ukprn = context.User.FindFirst(_ukprnClaimFinderPredicate).Value;
            var cookieKey = GetOncePerAuthorizedSessionCookieKey(ukprn);

            mvcContext.HttpContext.Response.Cookies.Append(cookieKey, "1", EsfaCookieOptions.GetDefaultHttpCookieOption(hostingEnvironment));
        }

        private static string GetOncePerAuthorizedSessionCookieKey(string ukprn)
        {
            return string.Format(CookieNames.SetupProvider, ukprn);
        }
    }
}