using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Events;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services.NextVacancyReview;
using Esfa.Recruit.Vacancies.Client.Application.Services.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Application.Services.VacancyComparer;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.EventStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BankHolidays;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BannedPhrases;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Profanities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Qualifications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BlockedEmployers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Skills;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Wages;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.SequenceStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.FAA;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Slack;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Recruit.Vacancies.Client.Infrastructure.Configuration;
using Recruit.Vacancies.Client.Infrastructure.Services.VacancyTitle;
using SFA.DAS.EAS.Account.Api.Client;
using VacancyRuleSet = Esfa.Recruit.Vacancies.Client.Application.Rules.VacancyRules.VacancyRuleSet;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRecruitStorageClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AccountApiConfiguration>(configuration.GetSection("AccountApiConfiguration"));

            services.AddMediatR(typeof(CreateVacancyCommandHandler).Assembly);
            services.AddTransient<IMessaging, MediatrMessaging>();
            
            // Vacancy Clients
            services.AddTransient<IEmployerVacancyClient, VacancyClient>();
            services.AddTransient<IJobsVacancyClient, VacancyClient>();
            services.AddTransient<IQaVacancyClient, QaVacancyClient>();

            services.RegisterServiceDeps(configuration);

            services.RegisterRepositories(configuration);

            services.AddTransient<IConfigurationReader, ConfigurationReader>();
            services.AddTransient<QaRulesConfiguration>(x => 
                                                            {
                                                                var svc = x.GetService<IConfigurationReader>();
                                                                return svc.GetAsync<QaRulesConfiguration>("QaRules").Result;
                                                            });

            services.RegisterStorageProviderDeps(configuration);

            services.AddValidation();

            services.AddRules();
        }

        private static void RegisterAccountApiClientDeps(this IServiceCollection services)
        {
            services.AddSingleton<IAccountApiConfiguration>(kernal => kernal.GetService<IOptions<AccountApiConfiguration>>().Value);
            services.AddTransient<IAccountApiClient, AccountApiClient>();
        }

        private static void RegisterServiceDeps(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuration
            services.Configure<GeocodeConfiguration>(configuration.GetSection("Geocode"));
            services.Configure<BankHolidayConfiguration>(configuration.GetSection("BankHoliday"));
            services.Configure<FaaConfiguration>(configuration.GetSection("FaaConfiguration"));
            services.Configure<VacancyApiConfiguration>(configuration.GetSection("VacancyApiConfiguration"));
            services.Configure<SlackConfiguration>(configuration.GetSection("Slack"));
            services.Configure<NextVacancyReviewServiceConfiguration>(o => o.VacancyReviewAssignationTimeoutMinutes = configuration.GetValue<int>("VacancyReviewAssignationTimeoutMinutes"));

            // Domain services
            services.AddTransient<ITimeProvider, CurrentUtcTimeProvider>();

            // Application Service
            services.AddTransient<IGenerateVacancyNumbers, MongoSequenceStore>();
            services.AddTransient<ISlaService, SlaService>();
            services.AddTransient<INotifyVacancyReviewUpdates, SlackNotifyVacancyReviewUpdates>();
            services.AddTransient<IVacancyService, VacancyService>();
            services.AddTransient<INextVacancyReviewService, NextVacancyReviewService>();
            services.AddTransient<IVacancyComparerService, VacancyComparerService>();
            services.AddTransient<IGetTitlePopularity, TitlePopularityService>();

            // Infrastructure Services
            services.AddTransient<IEmployerAccountProvider, EmployerAccountProvider>();
            services.AddTransient<ISlackClient, SlackClient>();
            services.AddTransient<IGeocodeServiceFactory, GeocodeServiceFactory>();
            services.AddTransient<IGetVacancyTitlesProvider, VacancyApiTitlesProvider>();

            // Projection services
            services.AddTransient<IEmployerDashboardProjectionService, EmployerDashboardProjectionService>();
            services.AddTransient<IQaDashboardProjectionService, QaDashboardProjectionService>();
            services.AddTransient<IEditVacancyInfoProjectionService, EditVacancyInfoProjectionService>();
            services.AddTransient<IPublishedVacancyProjectionService, PublishedVacancyProjectionService>();

            // Reference Data Providers
            services.AddTransient<IMinimumWageProvider, NationalMinimumWageProvider>();
            services.AddTransient<IApprenticeshipProgrammeProvider, ApprenticeshipProgrammeProvider>();
            services.AddTransient<IQualificationsProvider, QualificationsProvider>();
            services.AddTransient<ICandidateSkillsProvider, CandidateSkillsProvider>();
            services.AddTransient<IProfanityListProvider, ProfanityListProvider>();
            services.AddTransient<IBannedPhrasesProvider, BannedPhrasesProvider>();
            services.AddTransient<IBlockedEmployersProvider, BlockedEmployersProvider>();

            // Reference Data update services
            services.AddTransient<IApprenticeshipProgrammeUpdateService, ApprenticeshipProgrammeUpdateService>();
            services.AddTransient<IBankHolidayUpdateService, BankHolidayUpdateService>();
            services.AddTransient<IBankHolidayProvider, BankHolidayProvider>();
            
            // External client dependencies
            services.AddApprenticeshipsApi(configuration);
            services.RegisterAccountApiClientDeps();
        }

        private static void RegisterRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            var mongoConnectionString = configuration.GetConnectionString("MongoDb");
            
            services.Configure<MongoDbConnectionDetails>(options => 
            {
                options.ConnectionString = mongoConnectionString;
            });

            MongoDbConventions.RegisterMongoConventions();

            //Repositories
            services.AddTransient<IVacancyRepository, MongoDbVacancyRepository>();
            services.AddTransient<IVacancyReviewRepository, MongoDbVacancyReviewRepository>();
            services.AddTransient<IUserRepository, MongoDbUserRepository>();
            services.AddTransient<IApplicationReviewRepository, MongoDbApplicationReviewRepository>();
            services.AddTransient<IEmployerProfileRepository, MongoDbEmployerProfileRepository>();

            //Queries
            services.AddTransient<IVacancyQuery, MongoDbVacancyRepository>();
            services.AddTransient<IVacancyReviewQuery, MongoDbVacancyReviewRepository>();
            services.AddTransient<IApplicationReviewQuery, MongoDbApplicationReviewRepository>();

            services.AddTransient<IQueryStore, MongoQueryStore>();
            services.AddTransient<IQueryStoreReader, QueryStoreClient>();
            services.AddTransient<IQueryStoreWriter, QueryStoreClient>();

            services.AddTransient<IReferenceDataReader, MongoDbReferenceDataRepository>();
            services.AddTransient<IReferenceDataWriter, MongoDbReferenceDataRepository>();
        }

        private static void RegisterStorageProviderDeps(this IServiceCollection services, IConfiguration configuration)
        {
            var storageConnectionString = configuration.GetConnectionString("Storage");

            services.Configure<StorageQueueConnectionDetails>(options =>
            {
                options.ConnectionString = storageConnectionString;
            });

            services.AddSingleton(kernal => kernal.GetService<IOptions<StorageQueueConnectionDetails>>().Value);

            services.AddTransient<IEventStore, StorageQueueEventStore>();
        }

        private static void AddValidation(this IServiceCollection services)
        {
            services.AddTransient<AbstractValidator<Vacancy>, FluentVacancyValidator>();
            services.AddTransient(typeof(IEntityValidator<,>), typeof(EntityValidator<,>));

            services.AddSingleton<AbstractValidator<ApplicationReview>, ApplicationReviewValidator>();
            services.AddSingleton<AbstractValidator<VacancyReview>, VacancyReviewValidator>();
        }

        private static void AddRules(this IServiceCollection services)
        {
            services.AddTransient<RuleSet<Vacancy>, VacancyRuleSet>();
        }
    }
}
