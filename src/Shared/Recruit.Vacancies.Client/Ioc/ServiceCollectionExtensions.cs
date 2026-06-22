using System;
using Esfa.Recruit.Vacancies.Client.Application.Aspects;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Application.Events;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Application.Services.Reports;
using Esfa.Recruit.Vacancies.Client.Application.Services.VacancyComparer;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.EventStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BankHolidays;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BannedPhrases;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Profanities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Reports;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerProfile;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Locations;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Report;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProviderSummaryProvider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancyAnalytics;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Client.Infrastructure.User;
using Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
                .AddMemoryCache();
            RegisterClients(services);
            RegisterServiceDeps(services, configuration);
            RegisterAccountApiClientDeps(services);
            RegisterRepositories(services, configuration);
            RegisterOutOfProcessEventDelegatorDeps(services, configuration);
            RegisterQueueStorageServices(services, configuration);
            AddValidation(services);
            AddRules(services);
            RegisterMediatR(services);
            RegisterProviderRelationshipsClient(services);
        }

        private static void RegisterProviderRelationshipsClient(IServiceCollection services) => 
            services.AddTransient<IProviderRelationshipsService, ProviderRelationshipsService>();

        private static void RegisterAccountApiClientDeps(IServiceCollection services)
        {
            services.AddSingleton<IAccountApiConfiguration>(kernal => kernal.GetService<IOptions<AccountApiConfiguration>>().Value);
            services.AddTransient<IAccountApiClient, AccountApiClient>();
        }

        private static void RegisterServiceDeps(IServiceCollection services, IConfiguration configuration)
        {
            // Configuration
            services.AddSingleton(configuration);
            services.Configure<OuterApiConfiguration>(configuration.GetSection("OuterApiConfiguration"));

            // Domain services
            services.AddTransient<ITimeProvider, CurrentUtcTimeProvider>();

            // Application Service
            services.AddTransient<ISlaService, SlaService>();
            services.AddTransient<IVacancyService, VacancyService>();
            services.AddTransient<IVacancyComparerService, VacancyComparerService>();
            services.AddTransient<ICache, Cache>();
            services.AddTransient<IHtmlSanitizerService, HtmlSanitizerService>();
            services.AddTransient<IEmployerService, EmployerService>();
            services.AddHttpClient<IExternalWebsiteHealthCheckService, ExternalWebsiteHealthCheckService>();

            // Infrastructure Services
            services.AddTransient<IEmployerAccountProvider, EmployerAccountProvider>();
            services.AddTransient<ITrainingProviderService, TrainingProviderService>();
            services.AddTransient<IVacancyAnalyticsService, VacancyAnalyticsService>();
            services.AddTransient<ITrainingProviderSummaryProvider, TrainingProviderSummaryProvider>();
            services.AddHttpClient<IOuterApiClient, OuterApiClient>();
            services.AddTransient<IOuterApiGeocodeService, OuterApiGeocodeService>();
            services.AddTransient<ILocationsService, LocationsService>();
            services.AddTransient<IProviderReportService, ProviderReportService>();
            services.AddTransient<IEmployerProfileService, EmployerProfileService>();

            // Reference Data Providers
            services.AddTransient<IMinimumWageProvider, NationalMinimumWageProvider>();
            services.AddTransient<IApprenticeshipProgrammeProvider, ApprenticeshipProgrammeProvider>();
            services.AddTransient<IProfanityListProvider, ProfanityListProvider>();
            services.AddTransient<IBannedPhrasesProvider, BannedPhrasesProvider>();

            // Reference Data update services
            services.AddTransient<ITrainingProvidersUpdateService, TrainingProvidersUpdateService>();
            services.AddTransient<IBankHolidayProvider, BankHolidayProvider>();

        }

        private static void RegisterRepositories(IServiceCollection services, IConfiguration configuration)
        {
            //Repositories
            services.AddTransient<IVacancyRepository, SqlVacancyRepository>();
            services.AddTransient<IVacancyReviewRepository, VacancyReviewService>();
            services.AddTransient<IUserWriteRepository, UserService>();
            services.AddTransient<IUserRepository, UserService>();
            services.AddTransient<IApplicationWriteRepository, ApplicationReviewService>();
            services.AddTransient<IApplicationReadRepository, ApplicationReviewService>();
            services.AddTransient<IApplicationReviewRepositoryRunner, ApplicationReviewRepositoryRunner>();

            //Queries
            services.AddTransient<IVacancyQuery, SqlVacancyQuery>();
            services.AddTransient<IVacancyReviewQuery, VacancyReviewService>();
        }

        private static void RegisterOutOfProcessEventDelegatorDeps(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IEventStore, StorageQueueEventStore>();
        }

        private static void RegisterQueueStorageServices(IServiceCollection services, IConfiguration configuration)
        {
            var recruitStorageConnectionString = configuration.GetConnectionString("QueueStorage");
            services.AddTransient<IRecruitQueueService>(_ => new RecruitStorageQueueService(recruitStorageConnectionString));
        }

        private static void AddValidation(IServiceCollection services)
        {
            services.AddTransient<AbstractValidator<Vacancy>, FluentVacancyValidator>();
            services.AddTransient(typeof(IEntityValidator<,>), typeof(EntityValidator<,>));

            services.AddTransient<AbstractValidator<ApplicationReview>, ApplicationReviewValidator>();
            services.AddTransient<AbstractValidator<VacancyReview>, VacancyReviewValidator>();

            services.AddTransient<AbstractValidator<UserNotificationPreferences>, UserNotificationPreferencesValidator>();
            services.AddTransient<AbstractValidator<Qualification>, QualificationValidator>();
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
                .AddTransient<IJobsVacancyClient, VacancyClient>()
                .AddTransient<IGetAddressesClient, OuterApiGetAddressesClient>()
                .AddTransient<IGetProviderStatusClient, OuterApiGetProviderStatusClient>()
                .AddTransient<ILocationsClient, LocationsClient>()
                .AddTransient<IOuterApiVacancyClient, OuterApiVacancyClient>()
                .AddTransient<IReferenceDataClient, ReferenceDataClient>();
        }


        private static void RegisterMediatR(IServiceCollection services)
        {
            services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(CreateEmployerOwnedVacancyCommandHandler).Assembly));
            services
                .AddTransient<IMessaging, MediatrMessaging>()
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestPerformanceBehaviour<,>));
        }
    }
}