using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Events;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
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
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.SequenceStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.FAA;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.MinimumWage;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SFA.DAS.EAS.Account.Api.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Slack;

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
            
            services.AddTransient<IEmployerVacancyClient, VacancyClient>();
            services.AddTransient<IJobsVacancyClient, VacancyClient>();
            services.AddTransient<IQaVacancyClient, QaVacancyClient>();

            RegisterServiceDeps(services, configuration);

            services.AddRepositories(configuration);

            RegisterStorageProviderDeps(services, configuration);

            services.AddValidation();
        }

        private static void RegisterAccountApiClientDeps(IServiceCollection services)
        {
            services.AddSingleton<IAccountApiConfiguration>(kernal => kernal.GetService<IOptions<AccountApiConfiguration>>().Value);
            services.AddTransient<IAccountApiClient, AccountApiClient>();
        }

        private static void RegisterServiceDeps(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<ITimeProvider, CurrentUtcTimeProvider>();
            services.AddTransient<IEmployerAccountService, EmployerAccountService>();
            services.AddTransient<IDashboardService, DashboardService>();
            services.AddTransient<IGetMinimumWages, StubNationalMinimumWageService>();
            services.AddTransient<IGenerateVacancyNumbers, MongoSequenceStore>();
            services.AddTransient<IApprenticeshipProgrammeProvider, ApprenticeshipProgrammeProvider>();
            
            services.Configure<SlackConfiguration>(configuration.GetSection("Slack"));
            services.AddTransient<INotifyVacancyReviewUpdates, SlackNotifyVacancyReviewUpdates>();
            services.AddTransient<ISlackClient, SlackClient>();

            services.Configure<GeocodeConfiguration>(configuration.GetSection("Geocode"));
            services.AddTransient<IGeocodeServiceFactory, GeocodeServiceFactory>();

            services.Configure<FaaConfiguration>(configuration.GetSection("FaaConfiguration"));
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
            services.AddTransient<IVacancyReviewRepository, MongoDbVacancyReviewRepository>();
            services.AddTransient<IUserRepository, MongoDbUserRepository>();
            services.AddTransient<IApplicationReviewRepository, MongoDbApplicationReviewRepository>();

            services.AddTransient<IQueryStore, MongoQueryStore>();
            services.AddTransient<IQueryStoreReader, QueryStoreClient>();
            services.AddTransient<IQueryStoreWriter, QueryStoreClient>();

            services.AddTransient<IReferenceDataReader, MongoDbReferenceDataRepository>();
        }

        private static void RegisterStorageProviderDeps(IServiceCollection services, IConfiguration configuration)
        {
            var storageConnectionString = configuration.GetConnectionString("Storage");

            services.Configure<StorageQueueConnectionDetails>(options =>
            {
                options.ConnectionString = storageConnectionString;
            });

            services.AddSingleton(kernal => kernal.GetService<IOptions<StorageQueueConnectionDetails>>().Value);

            services.AddTransient<IEventStore, StorageQueueEventQueue>();
        }

        private static void AddValidation(this IServiceCollection services)
        {
            services.AddTransient<AbstractValidator<Vacancy>, FluentVacancyValidator>();
            services.AddTransient(typeof(IEntityValidator<,>), typeof(EntityValidator<,>));

            services.AddSingleton<AbstractValidator<ApplicationReview>, ApplicationReviewValidator>();
        }
    }
}
