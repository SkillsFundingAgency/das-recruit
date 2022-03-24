using System;
using System.Linq;
using Esfa.Recruit.Qa.Web.Configuration.Routing;
using FluentValidation.AspNetCore;
using Esfa.Recruit.Qa.Web.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.QA.Web.Filters;

namespace Esfa.Recruit.Qa.Web.Configuration
{
    public static class ServiceCollectionExtensions
    {
        private const int SessionTimeoutMinutes = 30;

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
                options.UseTokenLifetime = false;
                //options.CallbackPath = "/";
                //options.SkipUnrecognizedRequests = true;
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = CookieNames.QaData;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                options.AccessDeniedPath = RoutePaths.AccessDeniedPath;
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(SessionTimeoutMinutes);
            });
        }

        public static void AddAuthorizationService(this IServiceCollection services, AuthorizationConfiguration legacyAuthorizationConfig, AuthorizationConfiguration authorizationConfig)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthorizationPolicyNames.QaUserPolicyName, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(context => 
                        context.User.HasClaim(legacyAuthorizationConfig.ClaimType, legacyAuthorizationConfig.UserClaimValue) 
                        || context.User.HasClaim(legacyAuthorizationConfig.ClaimType, legacyAuthorizationConfig.TeamLeadClaimValue) 
                        || context.User.HasClaim(authorizationConfig.ClaimType, authorizationConfig.UserClaimValue)
                        || context.User.HasClaim(authorizationConfig.ClaimType, authorizationConfig.TeamLeadClaimValue)
                    );
                });
                options.AddPolicy(AuthorizationPolicyNames.TeamLeadUserPolicyName, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(context => 
                        context.User.HasClaim(legacyAuthorizationConfig.ClaimType, legacyAuthorizationConfig.TeamLeadClaimValue)
                        || context.User.HasClaim(authorizationConfig.ClaimType, authorizationConfig.TeamLeadClaimValue)
                    );
                });
            });
        }

        public static void AddMvcService(this IServiceCollection services, ILoggerFactory loggerFactory)
        {
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = CookieNames.AntiForgeryCookie;
                options.FormFieldName = "_csrfToken";
                options.HeaderName = "X-XSRF-TOKEN";
            });
            services.Configure<CookieTempDataProviderOptions>(options => options.Cookie.Name = CookieNames.QaTempData);
            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();

            services.AddMvc(options =>
                {
                    options.EnableEndpointRouting = false;
                    options.SslPort = 5025;
                    options.Filters.Add(new RequireHttpsAttribute());
                    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                    options.Filters.Add(new AuthorizeFilter(AuthorizationPolicyNames.QaUserPolicyName));
                    options.AddTrimModelBinderProvider(loggerFactory);

                    var jsonInputFormatters = options.InputFormatters.OfType<NewtonsoftJsonInputFormatter>();
                    foreach (var formatter in jsonInputFormatters)
                    {
                        formatter.SupportedMediaTypes
                            .Add(MediaTypeHeaderValue.Parse("application/csp-report"));
                    }

                    options.Filters.AddService<PlannedOutageResultFilter>();
            }).AddNewtonsoftJson()
            .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>())
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }
    }
}