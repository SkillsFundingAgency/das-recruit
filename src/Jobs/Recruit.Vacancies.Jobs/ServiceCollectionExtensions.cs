using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories;
using Esfa.Recruit.Vacancies.Client.Ioc;
using Esfa.Recruit.Vacancies.Jobs.AnalyticsSummaryProcessor;
using Esfa.Recruit.Vacancies.Jobs.Communication;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Application;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Candidate;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Employer;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Provider;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.VacancyReview;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.LiveVacancy;
using Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Communication.Core;
using Communication.Types.Interfaces;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Esfa.Recruit.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Application.Communications.EntityDataItemProviderPlugins;
using System.Collections.Generic;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using SFA.DAS.Encoding;
using Esfa.Recruit.Vacancies.Client.Application.Communications.ParticipantResolverPlugins;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Recruit.Vacancies.Client.Application.Communications.CompositeDataItemProviderPlugins;
using Esfa.Recruit.Vacancies.Jobs.Jobs;

namespace Esfa.Recruit.Vacancies.Jobs
{
    internal static class ServiceCollectionExtensions
    {
        public static void ConfigureJobServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRecruitStorageClient(configuration);

            // Add Jobs
            services.AddScoped<DomainEventsQueueTrigger>();
            services.AddScoped<VacancyStatusQueueTrigger>();
            services.AddScoped<GeneratePublishedVacanciesQueueTrigger>();
            services.AddScoped<UpdateBankHolidayQueueTrigger>();
            services.AddScoped<UpdateQaDashboardQueueTrigger>();
            services.AddScoped<TransferVacanciesFromProviderQueueTrigger>();
            services.AddScoped<TransferVacancyToLegalEntityQueueTrigger>();
            services.AddScoped<TransferVacanciesFromEmployerReviewToQAReviewQueueTrigger>();
            services.AddScoped<UpdateProvidersQueueTrigger>();

            services.AddScoped<TransferVacanciesFromEmployerReviewToQAReviewJob>();
            services.AddScoped<TransferVacanciesFromProviderJob>();
            services.AddScoped<TransferVacancyToLegalEntityJob>();

            services.AddScoped<INotificationService, NotificationService>();

            services.AddScoped<IAnalyticsAggregator, AnalyticsAggregator>();
            
            // Domain Event Queue Handlers

            // Vacancy
            services.AddScoped<IDomainEventHandler<IEvent>, DraftVacancyUpdatedHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, VacancyReferredDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, VacancyReviewedHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, VacancySubmittedHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, VacancyRejectedHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, ProviderBlockedOnVacancyDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, LiveVacancyChangedDateHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, LiveVacancyWithdrawnHandler>();

            // VacancyReview
            services.AddScoped<IDomainEventHandler<IEvent>, VacancyReviewApprovedHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, VacancyReviewReferredHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, VacancyReviewCreatedHandler>();

            // Application
            services.AddScoped<IDomainEventHandler<IEvent>, ApplicationSubmittedDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, ApplicationWithdrawnHandler>();

            // Employer
            services.AddScoped<IDomainEventHandler<IEvent>, SetupEmployerHandler>();

            // Provider
            services.AddScoped<IDomainEventHandler<IEvent>, SetupProviderHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, ProviderBlockedDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, ProviderBlockedOnLegalEntityDomainEventHandler>();

            //Candidate
            services.AddScoped<IDomainEventHandler<IEvent>, DeleteCandidateHandler>();

            RegisterCommunicationsService(services, configuration);
            RegisterDasEncodingService(services, configuration);

            services.AddSingleton<IFeature, Feature>();
        }

        public static void AddOpenTelemetryRegistration(this IServiceCollection services, string appInsightsConnectionString)
        {
            if (!string.IsNullOrEmpty(appInsightsConnectionString))
            {
                // This service will collect and send telemetry data to Azure Monitor.
                services.AddOpenTelemetry().UseAzureMonitor(options =>
                {
                    options.ConnectionString = appInsightsConnectionString;
                });
            }
        }
        
        private static void RegisterCommunicationsService(IServiceCollection services, IConfiguration configuration)
        {
            // Relies on services.AddRecruitStorageClient(configuration); being called first
            services.AddTransient<ICommunicationRepository, MongoDbCommunicationRepository>();

            services.AddScoped<CommunicationRequestQueueTrigger>();

            string communicationStorageConnString = configuration.GetConnectionString("CommunicationsStorage");
            services.AddSingleton<IDispatchQueuePublisher>(_ => new DispatchQueuePublisher(communicationStorageConnString));
            services.AddSingleton<IAggregateCommunicationComposeQueuePublisher>(_ => new AggregateCommunicationComposeQueuePublisher(communicationStorageConnString));
            services.AddScoped<CommunicationMessageDispatcherQueueTrigger>();
            services.AddScoped<CommunicationMessageDispatcher>();

            services.AddTransient<ICommunicationProcessor, CommunicationProcessor>();
            services.AddTransient<IAggregateCommunicationProcessor, AggregateCommunicationProcessor>();
            services.AddTransient<ICommunicationService, CommunicationService>();

            services.AddTransient<IParticipantResolver, VacancyParticipantsResolverPlugin>();
            services.AddTransient<IParticipantResolver, ProviderParticipantsResolverPlugin>();
            services.AddTransient<IParticipantResolver, EmployerParticipantsResolverPlugin>();
            services.AddTransient<IUserPreferencesProvider, UserPreferencesProviderPlugin>();
            services.AddTransient<ITemplateIdProvider, TemplateIdProviderPlugin>();
            services.AddTransient<IEntityDataItemProvider, VacancyDataEntityPlugin>();
            services.AddTransient<IEntityDataItemProvider, ApprenticeshipServiceUnsubscribeDataEntityPlugIn>();
            services.AddTransient<IEntityDataItemProvider, ApprenticeshipServiceUrlDataEntityPlugin>();
            services.AddTransient<IEntityDataItemProvider, ApprenticeshipServiceConfigDataEntityPlugin>();
            services.AddTransient<IEntityDataItemProvider, ProviderDataEntityPlugin>();
            services.AddTransient<IEntityDataItemProvider, EmployerDataEntityPlugin>();
            services.AddTransient<ICompositeDataItemProvider, ApplicationsSubmittedCompositeDataItemPlugin>();

            services.Configure<CommunicationsConfiguration>(configuration.GetSection("CommunicationsConfiguration"));
        }

        private static void RegisterDasEncodingService(IServiceCollection services, IConfiguration configuration)
        {
            var dasEncodingConfig = new EncodingConfig { Encodings = new List<Encoding>() };
            configuration.GetSection(nameof(dasEncodingConfig.Encodings)).Bind(dasEncodingConfig.Encodings);
            services.AddSingleton(dasEncodingConfig);
            services.AddSingleton<IEncodingService, EncodingService>();
        }
    }
}