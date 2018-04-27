using System.Linq;
using Esfa.Recruit.Qa.Web.Configuration.Routing;
using Esfa.Recruit.Qa.Web.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Esfa.Recruit.Qa.Web.Configuration
{
    public static class IServiceCollectionExtensions
    {
        private const string DoesUserBelongToGroupPolicyName = "DoesUserBelongToGroup";
        
        public static void AddAuthenticationService(this IServiceCollection services, AuthenticationConfiguration authConfig)
        {
            services.AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = WsFederationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultSignOutScheme = WsFederationDefaults.AuthenticationScheme;
                })
                .AddWsFederation(options =>
                {
                    options.Wtrealm = authConfig.Wtrealm;
                    options.MetadataAddress = authConfig.MetaDataAddress;
                    //options.CallbackPath = "/";
                    //options.SkipUnrecognizedRequests = true;
                })
                .AddCookie(options =>
                {
                    options.AccessDeniedPath = RoutePrefixPaths.AccessDeniedPath;
                });
        }

        public static void AddAuthorizationService(this IServiceCollection services, AuthorizationConfiguration authorizationConfig)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(DoesUserBelongToGroupPolicyName, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(authorizationConfig.GroupClaim, authorizationConfig.GroupName);
                });
            });
        }

        public static void AddMvcService(this IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.SslPort = 5025;
                options.Filters.Add(new RequireHttpsAttribute());
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                options.Filters.Add(new AuthorizeFilter(DoesUserBelongToGroupPolicyName));
                options.AddTrimModelBinderProvider();

                var jsonInputFormatters = options.InputFormatters.OfType<JsonInputFormatter>();
                foreach (var formatter in jsonInputFormatters)
                {
                    formatter.SupportedMediaTypes
                        .Add(MediaTypeHeaderValue.Parse("application/csp-report"));
                }
            });
        }
    }
}