using Employer.Web.Services;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Storage.Client.Core.Handlers;
using Esfa.Recruit.Storage.Client.Core.Messaging;
using Esfa.Recruit.Storage.Client.Core.Mongo;
using Esfa.Recruit.Storage.Client.Core.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Esfa.Recruit.Employer.Web.Configuration
{
    public static class IoC
    {
        public static void AddIoC(this IServiceCollection services, IConfiguration configuration)
        {
            //Configuration
            services.Configure<ExternalLinksConfiguration>(configuration.GetSection("ExternalLinks"));
            services.Configure<MongoDbConnectionDetails>(configuration.GetSection("MongoDbConnectionDetails"));

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // Used by NLog to log out traceidentifier value.

            services.AddTransient<IGetAssociatedEmployerAccountsService, GetAssociatedEmployerAccountsService>();

            //Mediatr
            services.AddMediatR(typeof(CreateVacancyCommandHandler).Assembly);
            services.AddTransient<IMessaging, MediatrMessaging>();

            //Orchestrators
            services.AddTransient<INewVacancyOrchestrator, NewVacancyOrchestrator>();

            //Repositories
            //todo: Add the mongo repo only if we have mongo configuration settings
            services.AddTransient<IVacancyRepository, MongoDbVacancyRepository>();
        }
    }
}
