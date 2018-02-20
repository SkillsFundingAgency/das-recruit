using Esfa.Recruit.Storage.Client.Core.Messaging;
using Esfa.Recruit.Storage.Client.Core.Mongo;
using Esfa.Recruit.Storage.Client.Core.Repositories;
using Esfa.Recruit.Storage.Client.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Esfa.Recruit.Storage.Client.Core.Entities;
using Esfa.Recruit.Storage.Client.Core.Handlers;

namespace Esfa.Recruit.Storage.Client.Core.Ioc
{
    public static class Registry
    {
        public static void AddRecruitStorageClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(typeof(UpsertVacancyCommandHandler).Assembly);
            services.AddTransient<IMessaging, MediatrMessaging>();
            
            services.AddTransient<IdGenerator, IdGenerator>();
            
            services.AddRepositories(configuration);
        }

        private static void AddRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection mongoConfig = configuration.GetSection("MongoDbConnectionDetails");
            services.Configure<MongoDbConnectionDetails>(mongoConfig);

            if (mongoConfig.Get<MongoDbConnectionDetails>() == null)
            {
                services.AddSingleton<ICommandVacancyRepository, StubVacancyRepository>();
                services.AddSingleton(x => (IQueryVacancyRepository)x.GetService<ICommandVacancyRepository>());
            }
            else
            {
                MongoDbConventions.RegisterMongoConventions();
                services.AddTransient<ICommandVacancyRepository, MongoDbVacancyRepository>();
                services.AddTransient<IQueryVacancyRepository, MongoDbVacancyRepository>();
            }
        }
    }
}
