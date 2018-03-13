using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.EAS.Account.Api.Client;
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
            services.Configure<AccountApiConfiguration>(configuration.GetSection("AccountApiConfiguration"));

            RegisterAccountApiClientDeps(services);
            RegisterProviderApiClientDep(services, configuration);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // Used by NLog to log out traceidentifier value.
            
            RegisterServiceDeps(services);

            RegisterOrchestratorDeps(services);
        }

        private static void RegisterServiceDeps(IServiceCollection services)
        {
            services.AddTransient<IEmployerAccountService, EmployerAccountService>();
            services.AddTransient<ITrainingProviderService, TrainingProviderService>();
        }

        private static void RegisterProviderApiClientDep(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IProviderApiClient>(_ => new ProviderApiClient(configuration.GetValue<string>("ProviderApiUrl")));
        }

        private static void RegisterAccountApiClientDeps(IServiceCollection services)
        {
            services.AddSingleton<IAccountApiConfiguration>(kernal => kernal.GetService<IOptions<AccountApiConfiguration>>().Value);
            services.AddTransient<IAccountApiClient, AccountApiClient>();
        }

        private static void RegisterOrchestratorDeps(IServiceCollection services)
        {
            services.AddTransient<ApplicationProcessOrchestrator>();
            services.AddTransient<ApprenticeshipDetailsOrchestrator>();
            services.AddTransient<CandidateProfileOrchestrator>();
            services.AddTransient<DashboardOrchestrator>();
            services.AddTransient<EmployerDetailsOrchestrator>();
            services.AddTransient<LocationOrchestrator>();
            services.AddTransient<TitleOrchestrator>();
            services.AddTransient<PreviewOrchestrator>();
            services.AddTransient<RoleDescriptionOrchestrator>();
            services.AddTransient<SectionsOrchestrator>();
            services.AddTransient<SubmittedOrchestrator>();
            services.AddTransient<TrainingProviderOrchestrator>();
            services.AddTransient<WageAndHoursOrchestrator>();
            services.AddTransient<DeleteVacancyOrchestrator>();
            services.AddTransient<ShortDescriptionOrchestrator>();
        }
    }
}