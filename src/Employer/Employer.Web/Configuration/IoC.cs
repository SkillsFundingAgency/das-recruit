using System;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Providers.Api.Client;

namespace Esfa.Recruit.Employer.Web.Configuration
{
    public static class IoC
    {
        public static void AddIoC(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRecruitStorageClient(configuration);

            //Configuration
            services.Configure<ExternalLinksConfiguration>(configuration.GetSection("ExternalLinks"));
            services.Configure<ManageApprenticeshipsRoutes>(configuration.GetSection("ManageApprenticeshipsRoutes"));
            services.AddSingleton<ManageApprenticeshipsLinkHelper>();
            services.Configure<AuthenticationConfiguration>(configuration.GetSection("Authentication"));
            services.Configure<SkillsConfiguration>(configuration.GetSection("SkillsConfiguration"));

            RegisterProviderApiClientDep(services, configuration);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // Used by NLog to log out traceidentifier value.
            
            RegisterServiceDeps(services, configuration);

            RegisterOrchestratorDeps(services);

            RegisterMapperDeps(services);
        }

        private static void RegisterServiceDeps(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<ITrainingProviderService, TrainingProviderService>();
            services.AddTransient<IGeocodeImageService>(_ => new GoogleMapsGeocodeImageService(configuration.GetValue<string>("GoogleMapsPrivateKey")));
        }

        private static void RegisterProviderApiClientDep(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IProviderApiClient>(_ => new ProviderApiClient(configuration.GetValue<string>("ProviderApiUrl")));
        }

        private static void RegisterOrchestratorDeps(IServiceCollection services)
        {
            services.AddTransient<AboutEmployerOrchestrator>();
            services.AddTransient<ApplicationProcessOrchestrator>();
            services.AddTransient<ConsiderationsOrchestrator>();
            services.AddTransient<DashboardOrchestrator>();
            services.AddTransient<EmployerOrchestrator>();
            services.AddTransient<TitleOrchestrator>();
            services.AddTransient<EntryPointOrchestrator>();
            services.AddTransient<VacancyPreviewOrchestrator>();
            services.AddTransient<SubmittedOrchestrator>();
            services.AddTransient<TrainingProviderOrchestrator>();
            services.AddTransient<DeleteVacancyOrchestrator>();
            services.AddTransient<ShortDescriptionOrchestrator>();
            services.AddTransient<TrainingOrchestrator>();
            services.AddTransient<VacancyDescriptionOrchestrator>();
            services.AddTransient<WageOrchestrator>();
            services.AddTransient<SearchResultPreviewOrchestrator>();
            services.AddTransient<SkillsOrchestrator>();
            services.AddTransient<QualificationsOrchestrator>();
            services.AddTransient<VacancyManageOrchestrator>();
        }

        private static void RegisterMapperDeps(IServiceCollection services)
        {
            services.AddTransient<DisplayVacancyViewModelMapper>();
        }
    }
}