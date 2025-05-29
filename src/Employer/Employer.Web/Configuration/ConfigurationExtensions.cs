using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System.Linq;
using Esfa.Recruit.Employer.Web.Middleware;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Employer.Web.Filters;
using Esfa.Recruit.Employer.Web.Interfaces;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.GovUK.Auth.Authentication;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.Configuration;

namespace Esfa.Recruit.Employer.Web.Configuration
{
    public static class ConfigurationExtensions
    {
        public static void AddAuthorizationService(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddTransient<IEmployerAccountAuthorizationHandler, EmployerAccountAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, EmployerAccountOwnerOrTransactorAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, EmployerAccountHandler>();
            services.AddTransient<IAuthorizationHandler, AccountActiveAuthorizationHandler>();
            
            services.AddAuthorization(options =>
            {
                // default authorization policy for all controller actions.
                options.AddPolicy(
                    PolicyNames.HasEmployerAccountPolicyName, policy =>
                    {
                        policy.Requirements.Add(new EmployerAccountRequirement());
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(new AccountActiveRequirement());
                    });
                // authorization policy for controller actions more specific for admin/owner roles.
                options.AddPolicy(
                    PolicyNames.HasEmployerOwnerOrTransactorAccount, policy =>
                    {
                        policy.Requirements.Add(new EmployerAccountOwnerOrTransactorRequirement());
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(new AccountActiveRequirement());
                    });
                    
            });
        }

        public static void AddMvcService(this IServiceCollection services, IWebHostEnvironment hostingEnvironment, bool isAuthEnabled, ILoggerFactory loggerFactory, IConfiguration configuration)
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
                    if (!hostingEnvironment.IsDevelopment())
                    {
                        opts.Filters.Add(new RequireHttpsAttribute());
                    }

                    if (!isAuthEnabled)
                    {
                        opts.Filters.Add(new AllowAnonymousFilter());
                    }
                    else
                    {
                        opts.Filters.Add(new AuthorizeFilter(PolicyNames.HasEmployerAccountPolicyName));
                    }

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
                }).SetDefaultNavigationSection(NavigationSection.RecruitHome)
                .AddNewtonsoftJson();
            services.AddFluentValidationAutoValidation();
            services.AddFeatureManagement(configuration.GetSection("Features"));
        }
    }
}