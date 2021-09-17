using System;
using Esfa.Recruit.Vacancies.Client.Application.Aspects;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Events;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services.NextVacancyReview;
using Esfa.Recruit.Vacancies.Client.Application.Services.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Application.Services.Reports;
using Esfa.Recruit.Vacancies.Client.Application.Services.VacancyComparer;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Configuration;
using Esfa.Recruit.Vacancies.Client.Infrastructure.EventStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BankHolidays;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BannedPhrases;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Profanities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Qualifications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Skills;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Reports;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.SequenceStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.FAA;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.PasAccount;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProviderSummaryProvider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Slack;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Client.Infrastructure.TableStore;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancyTitle;
using SFA.DAS.EAS.Account.Api.Client;
using VacancyRuleSet = Esfa.Recruit.Vacancies.Client.Application.Rules.VacancyRules.VacancyRuleSet;

namespace Esfa.Recruit.Vacancies.Client.Ioc
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRecruitStorageClient(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddHttpClient()
                .Configure<AccountApiConfiguration>(configuration.GetSection("AccountApiConfiguration"))
                .AddMemoryCache()
                .AddTransient<IConfigurationReader, ConfigurationReader>()
                .AddSingleton(x =>
                {
                    var svc = x.GetService<IConfigurationReader>();
                    return svc.GetAsync<QaRulesConfiguration>("QaRules").Result;
                });
            RegisterClients(services);
            RegisterServiceDeps(services, configuration);
            RegisterAccountApiClientDeps(services);
            RegisterTableStorageProviderDeps(services, configuration);
            RegisterRepositories(services, configuration);
            RegisterOutOfProcessEventDelegatorDeps(services, configuration);
            RegisterQueueStorageServices(services, configuration);
            AddValidation(services);
            AddRules(services);
            RegisterMediatR(services);
            RegisterProviderRelationshipsClient(services, configuration);
        }

        private static void RegisterProviderRelationshipsClient(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ProviderRelationshipApiConfiguration>(configuration.GetSection("ProviderRelationshipsApiConfiguration"));
            services.AddTransient<IProviderRelationshipsService, ProviderRelationshipsService>();
        }

        private static void RegisterAccountApiClientDeps(IServiceCollection services)
        {
            services.AddSingleton<IAccountApiConfiguration>(kernal => kernal.GetService<IOptions<AccountApiConfiguration>>().Value);
            services.AddTransient<IAccountApiClient, AccountApiClient>();
        }

        private static void RegisterServiceDeps(IServiceCollection services, IConfiguration configuration)
        {
            // Configuration
            services.Configure<GeocodeConfiguration>(configuration.GetSection("Geocode"));
            services.Configure<FaaConfiguration>(configuration.GetSection("FaaConfiguration"));
            services.Configure<VacancyApiConfiguration>(configuration.GetSection("VacancyApiConfiguration"));
            services.Configure<SlackConfiguration>(configuration.GetSection("Slack"));
            services.Configure<NextVacancyReviewServiceConfiguration>(o => o.VacancyReviewAssignationTimeoutMinutes = configuration.GetValue<int>("RecruitConfiguration:VacancyReviewAssignationTimeoutMinutes"));
            services.Configure<PasAccountApiConfiguration>(configuration.GetSection("PasAccountApiConfiguration"));
            services.Configure<OuterApiConfiguration>(configuration.GetSection("OuterApiConfiguration"));

            // Domain services
            services.AddTransient<ITimeProvider, CurrentUtcTimeProvider>();

            // Application Service
            services.AddTransient<IGenerateVacancyNumbers, MongoSequenceStore>();
            services.AddTransient<ISlaService, SlaService>();
            services.AddTransient<INotifyVacancyReviewUpdates, SlackNotifyVacancyReviewUpdates>();
            services.AddTransient<INotifyVacancyUpdates, SlackNotifyVacancyUpdates>();
            services.AddTransient<IVacancyService, VacancyService>();
            services.AddTransient<IVacancyTransferService, VacancyTransferService>();
            services.AddTransient<IVacancyReviewTransferService, VacancyReviewTransferService>();
            services.AddTransient<INextVacancyReviewService, NextVacancyReviewService>();
            services.AddTransient<IVacancyComparerService, VacancyComparerService>();
            services.AddTransient<IGetTitlePopularity, TitlePopularityService>();
            services.AddTransient<ICache, Cache>();
            services.AddTransient<IHtmlSanitizerService, HtmlSanitizerService>();
            services.AddTransient<IEmployerService, EmployerService>();

            //Reporting Service
            services.AddTransient<ICsvBuilder, CsvBuilder>();
            services.AddTransient<IReportService, ReportService>();
            services.AddTransient<ProviderApplicationsReportStrategy>();
            services.AddTransient<QaApplicationsReportStrategy>();
            services.AddTransient<Func<ReportType, IReportStrategy>>(serviceProvider => reportType =>
            {
                switch (reportType)
                {
                    case ReportType.ProviderApplications:
                        return serviceProvider.GetService<ProviderApplicationsReportStrategy>();
                    case ReportType.QaApplications:
                        return serviceProvider.GetService<QaApplicationsReportStrategy>();
                    default:
                        throw new Exception($"No report strategy for {reportType}");
                }
            });

            // Infrastructure Services
            services.AddTransient<IEmployerAccountProvider, EmployerAccountProvider>();
            services.AddTransient<ISlackClient, SlackClient>();
            services.AddTransient<IGeocodeServiceFactory, GeocodeServiceFactory>();
            services.AddTransient<IGetVacancyTitlesProvider, VacancyApiTitlesProvider>();
            services.AddTransient<ITrainingProviderService, TrainingProviderService>();
            services.AddTransient<ITrainingProviderSummaryProvider, TrainingProviderSummaryProvider>();
            services.AddTransient<IFaaService, FaaService>();
            services.AddTransient<IPasAccountProvider, PasAccountProvider>();
            services.AddHttpClient<IOuterApiClient, OuterApiClient>();

            // Projection services
            services.AddTransient<IEmployerDashboardProjectionService, EmployerDashboardProjectionService>();
            services.AddTransient<IProviderDashboardProjectionService, ProviderDashboardProjectionService>();
            services.AddTransient<IQaDashboardProjectionService, QaDashboardProjectionService>();
            services.AddTransient<IEditVacancyInfoProjectionService, EditVacancyInfoProjectionService>();
            services.AddTransient<IPublishedVacancyProjectionService, PublishedVacancyProjectionService>();
            services.AddTransient<IVacancyApplicationsProjectionService, VacancyApplicationsProjectionService>();
            services.AddTransient<IBlockedOrganisationsProjectionService, BlockedOrganisationsProjectionService>();

            // Reference Data Providers
            services.AddTransient<IMinimumWageProvider, NationalMinimumWageProvider>();
            services.AddTransient<IApprenticeshipProgrammeProvider, ApprenticeshipProgrammeProvider>();
            services.AddTransient<IQualificationsProvider, QualificationsProvider>();
            services.AddTransient<ICandidateSkillsProvider, CandidateSkillsProvider>();
            services.AddTransient<IProfanityListProvider, ProfanityListProvider>();
            services.AddTransient<IBannedPhrasesProvider, BannedPhrasesProvider>();

            // Query Data Providers
            services.AddTransient<IVacancySummariesProvider, VacancySummariesProvider>();

            // Reference Data update services
            services.AddTransient<IApprenticeshipProgrammesUpdateService, ApprenticeshipProgrammesUpdateService>();
            services.AddTransient<ITrainingProvidersUpdateService, TrainingProvidersUpdateService>();
            services.AddTransient<IBankHolidayUpdateService, BankHolidayUpdateService>();
            services.AddTransient<IBankHolidayProvider, BankHolidayProvider>();

        }

        private static void RegisterRepositories(IServiceCollection services, IConfiguration configuration)
        {
            var mongoConnectionString = configuration.GetConnectionString("MongoDb");

            services.Configure<MongoDbConnectionDetails>(options =>
            {
                options.ConnectionString = mongoConnectionString;
            });

            MongoDbConventions.RegisterMongoConventions();

            services.AddTransient<MongoDbCollectionChecker>();

            //Repositories
            services.AddTransient<IVacancyRepository, MongoDbVacancyRepository>();
            services.AddTransient<IVacancyReviewRepository, MongoDbVacancyReviewRepository>();
            services.AddTransient<IUserRepository, MongoDbUserRepository>();
            services.AddTransient<IApplicationReviewRepository, MongoDbApplicationReviewRepository>();
            services.AddTransient<IEmployerProfileRepository, MongoDbEmployerProfileRepository>();
            services.AddTransient<IReportRepository, MongoDbReportRepository>();
            services.AddTransient<IUserNotificationPreferencesRepository, MongoDbUserNotificationPreferencesRepository>();
            services.AddTransient<IBlockedOrganisationRepository, MongoDbBlockedOrganisationRepository>();

            //Queries
            services.AddTransient<IVacancyQuery, MongoDbVacancyRepository>();
            services.AddTransient<IVacancyReviewQuery, MongoDbVacancyReviewRepository>();
            services.AddTransient<IApplicationReviewQuery, MongoDbApplicationReviewRepository>();
            services.AddTransient<IBlockedOrganisationQuery, MongoDbBlockedOrganisationRepository>();

            services.AddTransient<IQueryStoreReader, QueryStoreClient>();
            services.AddTransient<IQueryStoreWriter, QueryStoreClient>();

            services.AddTransient<IReferenceDataReader, MongoDbReferenceDataRepository>();
            services.AddTransient<IReferenceDataWriter, MongoDbReferenceDataRepository>();
        }

        private static void RegisterOutOfProcessEventDelegatorDeps(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IEventStore, StorageQueueEventStore>();
        }

        private static void RegisterQueueStorageServices(IServiceCollection services, IConfiguration configuration)
        {
            var recruitStorageConnectionString = configuration.GetConnectionString("QueueStorage");
            var communicationStorageConnectionString = configuration.GetConnectionString("CommunicationsStorage");

            services.AddTransient<IRecruitQueueService>(_ => new RecruitStorageQueueService(recruitStorageConnectionString));
            services.AddTransient<ICommunicationQueueService>(_ => new CommunicationStorageQueueService(communicationStorageConnectionString));
        }

        private static void RegisterTableStorageProviderDeps(IServiceCollection services, IConfiguration configuration)
        {
            var storageConnectionString = configuration.GetConnectionString("TableStorage");
            var useTableStorageQueryStore = configuration.GetValue<bool>("RecruitConfiguration:UseTableStorageQueryStore");
            services.AddTransient<QueryStoreTableChecker>();
            services.Configure<TableStorageConnectionsDetails>(options =>
            {
                options.ConnectionString = storageConnectionString;
            });

            if (useTableStorageQueryStore)
                services.AddTransient<IQueryStore, TableStorageQueryStore>();
            else
            {
                services.AddTransient<IQueryStore, MongoQueryStore>();
                services.AddTransient<IQueryStoreHouseKeepingService, MongoQueryStore>();
            }
        }

        private static void AddValidation(IServiceCollection services)
        {
            services.AddSingleton<AbstractValidator<Vacancy>, FluentVacancyValidator>();
            services.AddSingleton(typeof(IEntityValidator<,>), typeof(EntityValidator<,>));

            services.AddSingleton<AbstractValidator<ApplicationReview>, ApplicationReviewValidator>();
            services.AddSingleton<AbstractValidator<VacancyReview>, VacancyReviewValidator>();

            services.AddSingleton<AbstractValidator<UserNotificationPreferences>, UserNotificationPreferencesValidator>();
            services.AddSingleton<AbstractValidator<Qualification>, QualificationValidator>();
        }

        private static void AddRules(IServiceCollection services)
        {
            services.AddTransient<RuleSet<Vacancy>, VacancyRuleSet>();
        }

        private static void RegisterClients(IServiceCollection services)
        {
            services
                .AddTransient<IRecruitVacancyClient, VacancyClient>()
                .AddTransient<IEmployerVacancyClient, VacancyClient>()
                .AddTransient<IProviderVacancyClient, VacancyClient>()
                .AddTransient<IQaVacancyClient, QaVacancyClient>()
                .AddTransient<IJobsVacancyClient, VacancyClient>();
        }


        private static void RegisterMediatR(IServiceCollection services)
        {
            services.AddMediatR(typeof(CreateEmployerOwnedVacancyCommandHandler).Assembly);
            services
                .AddTransient<IMessaging, MediatrMessaging>()
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestPerformanceBehaviour<,>));
        }
    }
}
