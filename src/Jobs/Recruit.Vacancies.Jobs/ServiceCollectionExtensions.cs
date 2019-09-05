using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.FAA;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories;
using Esfa.Recruit.Vacancies.Client.Ioc;
using Esfa.Recruit.Vacancies.Jobs.AnalyticsSummaryProcessor;
using Esfa.Recruit.Vacancies.Jobs.Communication;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Application;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Candidate;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Employer;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Provider;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.VacancyReview;
using Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.Http;
using SFA.DAS.Http.TokenGenerators;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Client.Configuration;
using Communication.Core;
using Communication.Types.Interfaces;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Esfa.Recruit.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Application.Communications.EntityDataItemProviderPlugins;
using System.Collections.Generic;
using SFA.DAS.Encoding;
using Esfa.Recruit.Vacancies.Client.Application.Communications.ParticipantResolverPlugins;
using Recruit.Vacancies.Client.Application.Communications.CompositeDataItemProviderPlugins;

namespace Esfa.Recruit.Vacancies.Jobs
{
    internal static class ServiceCollectionExtensions
    {
        public static void ConfigureJobServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IApprenticeshipProgrammeApiClient, ApprenticeshipProgrammeApiClient>();
            services.AddScoped(x => new AnalyticsEventStore(x.GetService<ILogger<AnalyticsEventStore>>(), configuration.GetConnectionString("VacancyAnalyticEventsSqlDbConnectionString")));

            services.AddRecruitStorageClient(configuration);

            services.AddSingleton<RecruitWebJobsSystemConfiguration>(x =>
                                                            {
                                                                var svc = x.GetService<IConfigurationReader>();
                                                                return svc.GetAsync<RecruitWebJobsSystemConfiguration>("RecruitWebJobsSystem").Result;
                                                            });

            // Add Jobs
            services.AddScoped<DomainEventsQueueTrigger>();
            services.AddScoped<UpdateApprenticeshipProgrammesQueueTrigger>();
            services.AddScoped<VacancyStatusQueueTrigger>();
            services.AddScoped<GenerateSingleEmployerDashboardQueueTrigger>();
            services.AddScoped<GeneratePublishedVacanciesQueueTrigger>();
            services.AddScoped<UpdateBankHolidayQueueTrigger>();
            services.AddScoped<UpdateQaDashboardQueueTrigger>();
            services.AddScoped<GenerateVacancyAnalyticsSummaryQueueTrigger>();
            services.AddScoped<TransferVacanciesFromProviderQueueTrigger>();
            services.AddScoped<TransferVacancyToLegalEntityQueueTrigger>();
            services.AddTransient<IFaaService, FaaService>();
#if DEBUG
            services.AddScoped<SpikeQueueTrigger>();
#endif

            services.AddScoped<TransferVacanciesFromProviderJob>();
            services.AddScoped<TransferVacancyToLegalEntityJob>();

            // Domain Event Queue Handlers

            // Vacancy
            services.AddScoped<IDomainEventHandler<IEvent>, DraftVacancyUpdatedHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, VacancyReferredDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, VacancySubmittedHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, ProviderBlockedOnVacancyDomainEventHandler>();

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
            RegisterDasNotifications(services, configuration);
            RegisterDasEncodingService(services, configuration);
        }

        private static void RegisterCommunicationsService(IServiceCollection services, IConfiguration configuration)
        {
            // Relies on services.AddRecruitStorageClient(configuration); being called first
            services.AddTransient<ICommunicationRepository, MongoDbCommunicationRepository>();

            services.AddScoped<CommunicationRequestQueueTrigger>();

            var communicationStorageConnString = configuration.GetConnectionString("CommunicationsStorage");
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
            services.AddTransient<IEntityDataItemProvider, ApprenticeshipServiceUrlDataEntityPlugin>();
            services.AddTransient<IEntityDataItemProvider, ApprenticeshipServiceConfigDataEntityPlugin>();
            services.AddTransient<IEntityDataItemProvider, ProviderDataEntityPlugin>();
            services.AddTransient<IEntityDataItemProvider, EmployerDataEntityPlugin>();
            services.AddTransient<ICompositeDataItemProvider, ApplicationsSubmittedCompositeDataItemPlugin>();

            services.Configure<CommunicationsConfiguration>(configuration.GetSection("CommunicationsConfiguration"));
        }

        private static void RegisterDasNotifications(IServiceCollection services, IConfiguration configuration)
        {
            var notificationsConfig = new NotificationsApiClientConfiguration();
            configuration.GetSection(nameof(NotificationsApiClientConfiguration)).Bind(notificationsConfig);

            var jwtToken = new JwtBearerTokenGenerator(notificationsConfig);

            var httpClient = new HttpClientBuilder()
                .WithBearerAuthorisationHeader(jwtToken)
                .WithDefaultHeaders()
                .Build();

            services.AddTransient<INotificationsApi>(sp => new NotificationsApi(httpClient, notificationsConfig));
        }

        private static void RegisterDasEncodingService(IServiceCollection services, IConfiguration configuration)
        {
            var dasEncodingConfig = new EncodingConfig { Encodings = new List<Encoding>() };
            configuration.GetSection(nameof(dasEncodingConfig.Encodings)).Bind(dasEncodingConfig.Encodings);
            services.AddSingleton<EncodingConfig>(dasEncodingConfig);
            services.AddSingleton<IEncodingService, EncodingService>();
        }
    }
}