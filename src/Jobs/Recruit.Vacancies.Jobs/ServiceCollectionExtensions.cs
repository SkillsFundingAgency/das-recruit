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
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.VacancyReview;
using Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers;
using Esfa.Recruit.Vacancies.Jobs.UpdateBlockedEmployers;
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

namespace Esfa.Recruit.Vacancies.Jobs
{
    internal static class ServiceCollectionExtensions
    {
        public static void ConfigureJobServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IApprenticeshipProgrammeApiClient, ApprenticeshipProgrammeApiClient>();
            services.AddScoped(x => new AccountsReader(x.GetService<ILogger<AccountsReader>>(), configuration.GetConnectionString("EmployerFinanceSqlDbConnectionString"), configuration.GetConnectionString("EmployerAccountsSqlDbConnectionString")));
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
            services.AddScoped<GenerateBlockedEmployersQueueTrigger>();
            services.AddScoped<GenerateVacancyAnalyticsSummaryQueueTrigger>();
            services.AddTransient<IFaaService, FaaService>();
#if DEBUG
            services.AddScoped<SpikeQueueTrigger>();
#endif

            // Domain Event Queue Handlers

            // Vacancy
            services.AddScoped<IDomainEventHandler<IEvent>, DraftVacancyUpdatedHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, VacancyReferredDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, VacancySubmittedHandler>();

            // VacancyReview
            services.AddScoped<IDomainEventHandler<IEvent>, VacancyReviewApprovedHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, VacancyReviewReferredHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, VacancyReviewCreatedHandler>();

            // Application
            services.AddScoped<IDomainEventHandler<IEvent>, ApplicationSubmittedHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, ApplicationWithdrawnHandler>();

            // Employer
            services.AddScoped<IDomainEventHandler<IEvent>, DomainEvents.Handlers.Employer.SetupEmployerHandler>();

            // Provider
            services.AddScoped<IDomainEventHandler<IEvent>, DomainEvents.Handlers.Provider.SetupProviderHandler>();

            //Candidate
            services.AddScoped<IDomainEventHandler<IEvent>, DeleteCandidateHandler>();

            RegisterCommunicationsService(services, configuration);
            RegisterDasNotifications(services, configuration);
        }

        private static void RegisterCommunicationsService(IServiceCollection services, IConfiguration configuration)
        {
            // Relies on services.AddRecruitStorageClient(configuration); being called first
            services.AddTransient<ICommunicationRepository, MongoDbCommunicationRepository>();

            services.AddScoped<CommunicationRequestQueueTrigger>();

            services.AddSingleton<IDispatchQueuePublisher>(_ => new DispatchQueuePublisher(configuration.GetConnectionString(("CommunicationsStorage"))));
            services.AddScoped<CommunicationMessageDispatcherQueueTrigger>();
            services.AddScoped<CommunicationMessageDispatcher>();

            services.AddTransient<ICommunicationProcessor, CommunicationProcessor>();
            services.AddTransient<ICommunicationService, CommunicationService>();

            services.AddTransient<IEntityDataItemProvider, VacancyEntityDataItemProviderPlugin>();
            services.AddTransient<IParticipantResolver, ParticipantResolverPlugin>();
            services.AddTransient<IUserPreferencesProvider, UserPreferencesProviderPlugin>();
            services.AddTransient<ITemplateIdProvider, TemplateIdProviderPlugin>();
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
    }
}