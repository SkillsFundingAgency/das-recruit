using Esfa.Recruit.Provider.Web.Filters;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Configuration;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.RuleTemplates;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.FAA;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;

namespace Esfa.Recruit.Provider.Web.Configuration
{
    public static class IoC
    {
        public static void AddIoC(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRecruitStorageClient(configuration);

            //Configuration
            services.Configure<ApplicationInsightsConfiguration>(configuration.GetSection("ApplicationInsights"));
            services.Configure<ExternalLinksConfiguration>(configuration.GetSection("ExternalLinks"));
            services.Configure<AuthenticationConfiguration>(configuration.GetSection("Authentication"));
            services.Configure<GoogleAnalyticsConfiguration>(configuration.GetSection("GoogleAnalytics"));
            services.Configure<PostcodeAnywhereConfiguration>(configuration.GetSection("PostcodeAnywhere"));
            services.Configure<FaaConfiguration>(configuration.GetSection("FaaConfiguration"));

            services.AddFeatureToggle();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // Used by NLog to log out traceidentifier value.
            
            RegisterServiceDeps(services, configuration);

            RegisterOrchestratorDeps(services);

            RegisterMapperDeps(services);

            RegisterFilterDeps(services);

            RegisterDynamicConfigurationDeps(services);
        }

        private static void RegisterServiceDeps(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IGeocodeImageService>(_ => new GoogleMapsGeocodeImageService(configuration.GetValue<string>("GoogleMapsPrivateKey")));
            services.AddTransient<IReviewSummaryService, ReviewSummaryService>();
        }

        private static void RegisterOrchestratorDeps(IServiceCollection services)
        {
            services.AddTransient<DashboardOrchestrator>();
            services.AddTransient<EmployerOrchestrator>();
            services.AddTransient<TitleOrchestrator>();
            services.AddTransient<SkillsOrchestrator>();
            services.AddTransient<VacancyDescriptionOrchestrator>();
        }

        private static void RegisterMapperDeps(IServiceCollection services)
        {
            services.AddTransient<ReviewFieldIndicatorMapper>();
            services.AddScoped<IRuleMessageTemplateRunner, RuleMessageTemplateRunner>();
        }

        private static void RegisterFilterDeps(IServiceCollection services)
        {
            services.AddScoped<PlannedOutageResultFilter>();
        }

        private static void RegisterDynamicConfigurationDeps(IServiceCollection services)
        {
            services.AddSingleton<ProviderRecruitSystemConfiguration>(x => 
                                                            {
                                                                var svc = x.GetService<IConfigurationReader>();
                                                                return svc.GetAsync<ProviderRecruitSystemConfiguration>("ProviderRecruitSystem").Result;
                                                            });
        }
    }
}