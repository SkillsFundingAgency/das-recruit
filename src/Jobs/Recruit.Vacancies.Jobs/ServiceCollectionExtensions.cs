using System.Collections.Generic;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Ioc;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Application;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Employer;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.LiveVacancy;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Provider;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.VacancyReview;
using Esfa.Recruit.Vacancies.Jobs.Jobs;
using Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Encoding;

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
            services.AddScoped<TransferVacanciesFromEmployerReviewToQAReviewQueueTrigger>();
            services.AddScoped<UpdateProvidersQueueTrigger>();
            services.AddScoped<TransferVacanciesFromEmployerReviewToQAReviewJob>();
            
            // Domain Event Queue Handlers

            // Vacancy
            services.AddScoped<IDomainEventHandler<IEvent>, DraftVacancyUpdatedHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, VacancyReviewedHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, VacancySubmittedHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, VacancyRejectedHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, LiveVacancyChangedDateHandler>();

            // VacancyReview
            services.AddScoped<IDomainEventHandler<IEvent>, VacancyReviewReferredHandler>();

            // Application
            services.AddScoped<IDomainEventHandler<IEvent>, ApplicationSubmittedDomainEventHandler>();

            // Employer
            services.AddScoped<IDomainEventHandler<IEvent>, SetupEmployerHandler>();

            // Provider
            services.AddScoped<IDomainEventHandler<IEvent>, SetupProviderHandler>();

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
        
        private static void RegisterDasEncodingService(IServiceCollection services, IConfiguration configuration)
        {
            var dasEncodingConfig = new EncodingConfig { Encodings = new List<Encoding>() };
            configuration.GetSection(nameof(dasEncodingConfig.Encodings)).Bind(dasEncodingConfig.Encodings);
            services.AddSingleton(dasEncodingConfig);
            services.AddSingleton<IEncodingService, EncodingService>();
        }
    }
}