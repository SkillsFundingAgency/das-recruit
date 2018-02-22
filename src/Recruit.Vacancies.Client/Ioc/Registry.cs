using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Esfa.Recruit.Storage.Client.Application.Handlers;
using Esfa.Recruit.Storage.Client.Domain.Messaging;
using Esfa.Recruit.Storage.Client.Infrastructure.Messaging;
using Esfa.Recruit.Storage.Client.Infrastructure.Mongo;
using Esfa.Recruit.Storage.Client.Domain.Repositories;
using Esfa.Recruit.Storage.Client.Infrastructure.Repositories;
using Esfa.Recruit.Storage.Client.Domain.QueryStore;
using Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Storage.Client.Core.Ioc
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
