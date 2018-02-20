using Employer.Web.Services;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Storage.Client.Core.Ioc;
using Esfa.Recruit.Storage.Client.Core.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Esfa.Recruit.Employer.Web.Configuration
{
    public static class IoC
    {
        public static void AddIoC(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRecruitStorageClient(configuration);

            //Configuration
            services.Configure<ExternalLinksConfiguration>(configuration.GetSection("ExternalLinks"));
            
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // Used by NLog to log out traceidentifier value.

            services.AddTransient<IGetAssociatedEmployerAccountsService, GetAssociatedEmployerAccountsService>();

            //Orchestrators
            services.AddTransient<ApplicationProcessOrchestrator, ApplicationProcessOrchestrator>();
            services.AddTransient<ApprenticeshipDetailsOrchestrator, ApprenticeshipDetailsOrchestrator>();
            services.AddTransient<CandidateProfileOrchestrator, CandidateProfileOrchestrator>();
            services.AddTransient<EmployerDetailsOrchestrator, EmployerDetailsOrchestrator>();
            services.AddTransient<LocationAndPositionsOrchestrator, LocationAndPositionsOrchestrator>();
            services.AddTransient<CreateVacancyOrchestrator, CreateVacancyOrchestrator>();
            services.AddTransient<PreviewOrchestrator, PreviewOrchestrator>();
            services.AddTransient<RoleDescriptionOrchestrator, RoleDescriptionOrchestrator>();
            services.AddTransient<SectionsOrchestrator, SectionsOrchestrator>();
            services.AddTransient<SubmittedOrchestrator, SubmittedOrchestrator>();
            services.AddTransient<TrainingProviderOrchestrator, TrainingProviderOrchestrator>();
            services.AddTransient<WageAndHoursOrchestrator, WageAndHoursOrchestrator>();
        }
    }
}
