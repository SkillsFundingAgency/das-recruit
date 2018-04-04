using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Events;
using Esfa.Recruit.Vacancies.Client.Application.Handlers;
using Esfa.Recruit.Vacancies.Client.Application.QueryStore;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SFA.DAS.EAS.Account.Api.Client;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRecruitStorageClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AccountApiConfiguration>(configuration.GetSection("AccountApiConfiguration"));
            services.Configure<QualificationsConfiguration>(configuration.GetSection("QualificationsConfiguration"));

            RegisterAccountApiClientDeps(services);

            services.AddMediatR(typeof(CreateVacancyCommandHandler).Assembly);
            services.AddTransient<IMessaging, MediatrMessaging>();
            
            services.AddTransient<IVacancyClient, VacancyClient>();

            RegisterServiceDeps(services);

            services.AddRepositories(configuration);

            RegisterStorageProviderDeps(services, configuration);

            services.AddValidation();
        }

        private static void RegisterAccountApiClientDeps(IServiceCollection services)
        {
            services.AddSingleton<IAccountApiConfiguration>(kernal => kernal.GetService<IOptions<AccountApiConfiguration>>().Value);
            services.AddTransient<IAccountApiClient, AccountApiClient>();
        }

        private static void RegisterServiceDeps(IServiceCollection services)
        {
            services.AddTransient<ITimeProvider, CurrentUtcTimeProvider>();
            services.AddTransient<IEmployerAccountService, EmployerAccountService>();
            services.AddTransient<IGetApprenticeNationalMinimumWages, StubNationalMinimumWageService>();
            services.AddTransient<ICreateDashboards, DashboardService>();
        }

        private static void AddRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            var mongoConnectionString = configuration.GetConnectionString("MongoDb");
            
            services.Configure<MongoDbConnectionDetails>(options => 
            {
                options.ConnectionString = mongoConnectionString;
            });

            MongoDbConventions.RegisterMongoConventions();
            services.AddTransient<IVacancyRepository, MongoDbVacancyRepository>();
            services.AddTransient<IQueryStore, MongoQueryStore>();
            services.AddTransient<IQueryStoreReader, QueryStoreClient>();
  
            services.AddTransient<IQueryStoreWriter, QueryStoreClient>();
        }

        private static void RegisterStorageProviderDeps(IServiceCollection services, IConfiguration configuration)
        {
            var storageConnectionString = configuration.GetConnectionString("Storage");

            services.Configure<StorageQueueConnectionDetails>(options =>
            {
                options.ConnectionString = storageConnectionString;
            });

            services.AddSingleton<StorageQueueConnectionDetails>(kernal => kernal.GetService<IOptions<StorageQueueConnectionDetails>>().Value);

            services.AddTransient<IEventStore, StorageQueueEventQueue>();
        }

        private static void AddValidation(this IServiceCollection services)
        {
            services.AddTransient<AbstractValidator<Vacancy>, FluentVacancyValidator>();
            services.AddTransient(typeof(IEntityValidator<,>), typeof(EntityValidator<,>));
        }
    }
}
