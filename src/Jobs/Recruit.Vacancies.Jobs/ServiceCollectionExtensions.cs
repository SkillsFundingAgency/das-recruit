using System;
using Communication.Core;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
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

            RegisterCommunicationsService(services);
        }

        private static void RegisterCommunicationsService(IServiceCollection services)
        {
            // Relies on services.AddRecruitStorageClient(configuration); being called first
            services.AddTransient<ICommunicationRepository, MongoDbCommunicationRepository>();
            services.AddTransient<ICommunicationProcessor, CommunicationProcessor>();
            services.AddTransient<ICommunicationService, CommunicationService>();

            services.AddScoped<CommunicationRequestQueueTrigger>();
        }
    }
}
