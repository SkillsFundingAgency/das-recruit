using Esfa.Recruit.Vacancies.Client.Application.Handlers;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.QueryStore;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Esfa.Recruit.Vacancies.Client.Ioc
{
    public static class Registry
    {
        public static void AddRecruitStorageClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(typeof(CreateVacancyCommandHandler).Assembly);
            services.AddTransient<IMessaging, MediatrMessaging>();
            
            services.AddTransient<IVacancyClient, VacancyClient>();
            
            services.AddRepositories(configuration);
        }

        private static void AddRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            var mongoConnectionString = configuration.GetConnectionString("MongoDb");
            
            services.Configure<MongoDbConnectionDetails>(options => 
            {
                options.ConnectionString = mongoConnectionString;
            });
            
            if (string.IsNullOrWhiteSpace(mongoConnectionString))
            {
                services.AddSingleton<IVacancyRepository, StubVacancyRepository>();
                services.AddSingleton<IQueryStoreReader, StubQueryStore>();
            }
            else
            {
                MongoDbConventions.RegisterMongoConventions();
                services.AddTransient<IVacancyRepository, MongoDbVacancyRepository>();
                services.AddTransient<IQueryStoreReader, QueryStore>();
            }
        }
    }
}
