using System;
using System.Linq;
using Esfa.Recruit.Qa.Web.Configuration.Routing;
using Esfa.Recruit.Qa.Web.Security;
using Esfa.Recruit.QA.Web.Filters;
using Esfa.Recruit.Shared.Web.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using SFA.DAS.DfESignIn.Auth.AppStart;
using SFA.DAS.DfESignIn.Auth.Constants;

namespace Esfa.Recruit.Qa.Web.Configuration
{
    public static class ServiceCollectionExtensions
    {
        private const int SessionTimeoutMinutes = 30;
        private const string DfESignInClientName = "QA";

        public static void AddAuthenticationService(this IServiceCollection services, AuthenticationConfiguration authConfig, IConfiguration config)
        {
            // condition to check if the DfeSignIn is allowed.
            bool isDfESignInAllowed = config.GetValue<bool>("UseDfeSignIn");
            if (isDfESignInAllowed)
            {
                // register DfeSignIn authentication services to the AspNetCore Authentication Options.
                services.AddAndConfigureDfESignInAuthentication(config, $"{CookieNames.QaData}", typeof(CustomServiceRole), DfESignInClientName);
            }
            else
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
                        // including the DfESignIn service role URI in the authorization policy.
                        || context.User.HasClaim(CustomClaimsIdentity.Service, authorizationConfig.UserClaimValue) 
                        || context.User.HasClaim(CustomClaimsIdentity.Service, authorizationConfig.TeamLeadClaimValue)
                    );
                });
                options.AddPolicy(AuthorizationPolicyNames.TeamLeadUserPolicyName, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(legacyAuthorizationConfig.ClaimType, legacyAuthorizationConfig.TeamLeadClaimValue)
                        || context.User.HasClaim(authorizationConfig.ClaimType, authorizationConfig.TeamLeadClaimValue)
                        // including the DfESignIn service role URI in the authorization policy.
                        || context.User.HasClaim(CustomClaimsIdentity.Service, authorizationConfig.TeamLeadClaimValue)
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
                }).AddNewtonsoftJson();
            services.AddValidatorsFromAssemblyContaining<Startup>();
        }
    }
}