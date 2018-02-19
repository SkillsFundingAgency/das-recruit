using Employer.Web.Services;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Storage.Client.Core.Ioc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Esfa.Recruit.Employer.Web.Configuration
{
    public static class IoC
    {
        public static void AddIoC(this IServiceCollection services, IConfiguration configuration)
        {
            //Recruit.Storage.Client IoC
            services.AddRecruitStorageClient(configuration);

            //Configuration
            services.Configure<ExternalLinksConfiguration>(configuration.GetSection("ExternalLinks"));
            
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // Used by NLog to log out traceidentifier value.

            services.AddTransient<IGetAssociatedEmployerAccountsService, GetAssociatedEmployerAccountsService>();
            
            //Orchestrators
            services.AddTransient<NewVacancyOrchestrator, NewVacancyOrchestrator>();
        }
    }
}
