﻿using Esfa.Recruit.Qa.Web.Configuration;
using Esfa.Recruit.Qa.Web.Mappings;
using Esfa.Recruit.Qa.Web.Orchestrators;
using Esfa.Recruit.Qa.Web.Security;
using Esfa.Recruit.Shared.Web.Configuration;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.RuleTemplates;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Ioc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Qa.Web
{
    public partial class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly AuthenticationConfiguration _authenticationConfig;
        private readonly AuthorizationConfiguration _legacyAuthorizationConfig;
        private readonly AuthorizationConfiguration _authorizationConfig;
        private readonly ExternalLinksConfiguration _externalLinks;
        private readonly ILoggerFactory _loggerFactory;

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _authenticationConfig = _configuration.GetSection("Authentication").Get<AuthenticationConfiguration>();
            _legacyAuthorizationConfig = _configuration.GetSection("LegacyAuthorization").Get<AuthorizationConfiguration>();
            _authorizationConfig = _configuration.GetSection("Authorization").Get<AuthorizationConfiguration>();
            _externalLinks = _configuration.GetSection("ExternalLinks").Get<ExternalLinksConfiguration>();
            _loggerFactory = loggerFactory;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ApplicationInsightsConfiguration>(_configuration.GetSection("ApplicationInsights"));

            //A service provider for resolving services configured in IoC
            var sp = services.BuildServiceProvider();

            // Routing has to come before adding Mvc
            services.AddRouting(opt =>
            {
                opt.AppendTrailingSlash = true;
            });

            services.AddMvcService(_loggerFactory);

            services.AddAntiforgery(
                options =>
                {
                    options.Cookie.Name = CookieNames.AntiForgeryCookie;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.HeaderName = "X-XSRF-TOKEN";
                }
            );

            services.AddApplicationInsightsTelemetry(_configuration);
            services.AddAuthenticationService(_authenticationConfig);
            services.AddAuthorizationService(_legacyAuthorizationConfig, _authorizationConfig);

            services.AddRecruitStorageClient(_configuration);
            services.AddScoped<DashboardOrchestrator>();
            services.AddScoped<ReviewOrchestrator>();
            services.AddTransient<UserAuthorizationService>();

            services.AddTransient<IGeocodeImageService>(_ => new GoogleMapsGeocodeImageService(_configuration.GetValue<string>("GoogleMapsPrivateKey")));
            services.AddScoped<ReviewMapper>();
            services.AddTransient<IReviewSummaryService, ReviewSummaryService>();
            services.AddTransient<ReviewFieldIndicatorMapper>();

            services.AddScoped<IRuleMessageTemplateRunner, RuleMessageTemplateRunner>();
        }
    }
}
