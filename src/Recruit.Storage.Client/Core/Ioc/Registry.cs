using Esfa.Recruit.Storage.Client.Core.Handlers;
using Esfa.Recruit.Storage.Client.Core.Messaging;
using Esfa.Recruit.Storage.Client.Core.Mongo;
using Esfa.Recruit.Storage.Client.Core.Repositories;
using Esfa.Recruit.Storage.Client.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace Esfa.Recruit.Storage.Client.Core.Ioc
{
    public static class Registry
    {
        public static void RegisterRecruitStorageClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(typeof(CreateVacancyCommandHandler).Assembly);
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
                services.AddSingleton<IVacancyRepository, StubVacancyRepository>();
            }
            else
            {
                MongoDbVacancyRepository.RegisterMongoMappings();
                services.AddTransient<IVacancyRepository, MongoDbVacancyRepository>();
            }
        }
    }
}
