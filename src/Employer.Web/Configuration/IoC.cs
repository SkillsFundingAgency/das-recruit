using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Storage.Client.Core.Ioc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.EAS.Account.Api.Client;

namespace Esfa.Recruit.Employer.Web.Configuration
{
    public static class IoC
    {
        public static void AddIoC(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRecruitStorageClient(configuration);

            //Configuration
            services.Configure<ExternalLinksConfiguration>(configuration.GetSection("ExternalLinks"));
            services.Configure<AuthenticationConfiguration>(configuration.GetSection("Authentication"));
            services.Configure<AccountApiConfiguration>(configuration.GetSection("AccountApiConfiguration"));
            
            RegisterAccountApiClientDeps(services);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // Used by NLog to log out traceidentifier value.

            if (configuration.GetValue<bool>("UseStubs"))
            {
                services.AddTransient<IEmployerAccountService, StubEmployerAccountService>();
            }
            else
            {
                services.AddTransient<IEmployerAccountService, EmployerAccountService>();
            }

            RegisterOrchestratorDeps(services);
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
            services.AddTransient<LocationAndPositionsOrchestrator>();
            services.AddTransient<CreateVacancyOrchestrator>();
            services.AddTransient<PreviewOrchestrator>();
            services.AddTransient<RoleDescriptionOrchestrator>();
            services.AddTransient<SectionsOrchestrator>();
            services.AddTransient<SubmittedOrchestrator>();
            services.AddTransient<TrainingProviderOrchestrator>();
            services.AddTransient<WageAndHoursOrchestrator>();
        }
    }
}