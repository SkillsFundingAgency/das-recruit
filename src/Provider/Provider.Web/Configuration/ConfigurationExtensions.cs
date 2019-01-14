using System;
using System.Linq;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Provider.Web.Filters;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Esfa.Recruit.Provider.Web.Configuration
{
    public static class ConfigurationExtensions
    {
        private const int SessionTimeoutMinutes = 30;

        private const string ProviderPolicyName = "ProviderPolicy";

        public static void AddAuthorizationService(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(ProviderPolicyName, policy =>
                {
                    policy.RequireAuthenticatedUser();
                });
            });
        }

        public static void AddMvcService(this IServiceCollection services, IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory)
        {
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = CookieNames.AntiForgeryCookie;
                options.FormFieldName = "_csrfToken";
                options.HeaderName = "X-XSRF-TOKEN";
            });
            services.Configure<CookieTempDataProviderOptions>(options => options.Cookie.Name = CookieNames.RecruitTempData);
            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddMvc(opts =>
            {
                opts.Filters.Add(new AuthorizeFilter(ProviderPolicyName));

                var jsonInputFormatters = opts.InputFormatters.OfType<JsonInputFormatter>();
                foreach (var formatter in jsonInputFormatters)
                {
                    formatter.SupportedMediaTypes
                        .Add(MediaTypeHeaderValue.Parse("application/csp-report"));
                }

                opts.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());

                opts.Filters.AddService<PlannedOutageResultFilter>();

                opts.AddTrimModelBinderProvider(loggerFactory);
            })
            .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>())
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public static void AddAuthenticationService(this IServiceCollection services, AuthenticationConfiguration authConfig, IEmployerVacancyClient vacancyClient, IHostingEnvironment hostingEnvironment)
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
                options.Wtrealm = authConfig.WtRealm;
                options.MetadataAddress = authConfig.MetaDataAddress;
                options.UseTokenLifetime = false;
                //options.CallbackPath = "/";
                //options.SkipUnrecognizedRequests = true;
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = CookieNames.RecruitData;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                options.AccessDeniedPath = RoutePaths.AccessDeniedPath;
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(SessionTimeoutMinutes);
            });
        }
    }
}