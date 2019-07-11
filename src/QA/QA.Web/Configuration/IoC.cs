using Esfa.Recruit.Qa.Web.Mappings;
using Esfa.Recruit.Qa.Web.Orchestrators;
using Esfa.Recruit.Qa.Web.Orchestrators.Reports;
using Esfa.Recruit.Qa.Web.Security;
using Esfa.Recruit.QA.Web.Configuration;
using Esfa.Recruit.QA.Web.Filters;
using Esfa.Recruit.Shared.Web.Configuration;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.RuleTemplates;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Ioc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Esfa.Recruit.Qa.Web.Configuration
{
    public static class IoC
    {
        public static void AddIoC(this IServiceCollection services, IConfiguration configuration)
        {
            var authenticationConfig = configuration.GetSection("Authentication").Get<AuthenticationConfiguration>();
            var legacyAuthorizationConfig = configuration.GetSection("LegacyAuthorization").Get<AuthorizationConfiguration>();
            var authorizationConfig = configuration.GetSection("Authorization").Get<AuthorizationConfiguration>();
            var externalLinks = configuration.GetSection("ExternalLinks").Get<ExternalLinksConfiguration>();

            services.Configure<ApplicationInsightsConfiguration>(configuration.GetSection("ApplicationInsights"));
            services.AddApplicationInsightsTelemetry(configuration);
            services.AddAuthenticationService(authenticationConfig);
            services.AddAuthorizationService(legacyAuthorizationConfig, authorizationConfig);

            services.AddRecruitStorageClient(configuration);
            services.AddTransient<UserAuthorizationService>();

            services.AddTransient<IGeocodeImageService>(_ => new GoogleMapsGeocodeImageService(configuration.GetValue<string>("GoogleMapsPrivateKey")));
            services.AddScoped<ReviewMapper>();
            services.AddTransient<IReviewSummaryService, ReviewSummaryService>();
            services.AddTransient<ReviewFieldIndicatorMapper>();

            services.AddScoped<IRuleMessageTemplateRunner, RuleMessageTemplateRunner>();

            services.AddScoped<PlannedOutageResultFilter>();

            services.AddSingleton(x =>
            {
                var svc = x.GetService<IConfigurationReader>();
                return svc.GetAsync<QaRecruitSystemConfiguration>("QaRecruitSystem").Result;
            });

            RegisterOrchestrators(services);
        }

        private static void RegisterOrchestrators(IServiceCollection services)
        {
            services.AddScoped<DashboardOrchestrator>();
            services.AddScoped<ReviewOrchestrator>();
            services.AddTransient<ReportDashboardOrchestrator>();
            services.AddTransient<ApplicationsReportOrchestrator>();
            services.AddTransient<ReportConfirmationOrchestrator>();
        }
    }
}
