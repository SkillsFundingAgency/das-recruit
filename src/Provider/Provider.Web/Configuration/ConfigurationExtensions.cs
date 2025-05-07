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
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Esfa.Recruit.Provider.Web.Middleware;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Provider.Shared.UI;
using SFA.DAS.Provider.Shared.UI.Startup;
using Microsoft.FeatureManagement;

namespace Esfa.Recruit.Provider.Web.Configuration
{
    public static class ConfigurationExtensions
    {
        private const int SessionTimeoutMinutes = 30;

        public static void AddAuthorizationService(this IServiceCollection services)
        {
            var ukPrnClaimName = ProviderRecruitClaims.DfEUkprnClaimsTypeIdentifier;
            var serviceClaimName = ProviderRecruitClaims.DfEUserServiceTypeClaimTypeIdentifier;
            
            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyNames.ProviderPolicyName, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(ukPrnClaimName);
                    policy.RequireClaim(serviceClaimName);
                    policy.Requirements.Add(new ProviderAccountRequirement());
                    policy.Requirements.Add(new TrainingProviderAllRolesRequirement());
                });

                options.AddPolicy(PolicyNames.HasContributorOrAbovePermission, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(ukPrnClaimName);
                    policy.RequireClaim(serviceClaimName);
                    policy.Requirements.Add(new MinimumServiceClaimRequirement(ServiceClaim.DAC));
                    policy.Requirements.Add(new TrainingProviderAllRolesRequirement());
                });

                options.AddPolicy(PolicyNames.HasContributorWithApprovalOrAbovePermission, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(ukPrnClaimName);
                    policy.RequireClaim(serviceClaimName);
                    policy.Requirements.Add(new MinimumServiceClaimRequirement(ServiceClaim.DAB));
                    policy.Requirements.Add(new TrainingProviderAllRolesRequirement());
                });

                options.AddPolicy(PolicyNames.HasAccountOwnerPermission, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(ukPrnClaimName);
                    policy.RequireClaim(serviceClaimName);
                    policy.Requirements.Add(new MinimumServiceClaimRequirement(ServiceClaim.DAA));
                    policy.Requirements.Add(new TrainingProviderAllRolesRequirement());
                });
            });

            services.AddTransient<IAuthorizationHandler, ProviderAccountHandler>();
            services.AddTransient<IAuthorizationHandler, MinimumServiceClaimRequirementHandler>();
            services.AddSingleton<ITrainingProviderAuthorizationHandler, TrainingProviderAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, TrainingProviderAllRolesAuthorizationHandler>();
        }

        public static void AddMvcService(this IServiceCollection services, IWebHostEnvironment hostingEnvironment, ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = CookieNames.AntiForgeryCookie;
                options.FormFieldName = "_csrfToken";
                options.HeaderName = "X-XSRF-TOKEN";
            });
            services.Configure<CookieTempDataProviderOptions>(options => options.Cookie.Name = CookieNames.RecruitTempData);
            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
            services.AddMvc(opts =>
                {
                    opts.EnableEndpointRouting = false;
                    opts.Filters.Add(new AuthorizeFilter(PolicyNames.ProviderPolicyName));

                    var jsonInputFormatters = opts.InputFormatters.OfType<NewtonsoftJsonInputFormatter>();
                    foreach (var formatter in jsonInputFormatters)
                    {
                        formatter.SupportedMediaTypes
                            .Add(MediaTypeHeaderValue.Parse("application/csp-report"));
                    }

                    opts.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());

                    opts.Filters.AddService<GoogleAnalyticsFilter>();
                    opts.Filters.AddService<ZendeskApiFilter>();

                    opts.AddTrimModelBinderProvider(loggerFactory);
                }
            ).AddNewtonsoftJson()
            .EnableCookieBanner()
            .EnableGoogleAnalytics()
            .SetDefaultNavigationSection(NavigationSection.Recruit);
            services.AddFluentValidationAutoValidation();
            services.AddFeatureManagement(configuration.GetSection("Features"));
        }
    }
}