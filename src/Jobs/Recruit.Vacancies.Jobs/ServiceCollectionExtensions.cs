using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Jobs.ApprenticeshipProgrammes;
using Esfa.Recruit.Vacancies.Jobs.BankHoliday;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Application;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Candidate;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Vacancy;
using Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.VacancyReview;
using Esfa.Recruit.Vacancies.Jobs.EmployerDashboardGenerator;
using Esfa.Recruit.Vacancies.Jobs.NonLevyAccountBlocker;
using Esfa.Recruit.Vacancies.Jobs.PublishedVacanciesGenerator;
using Esfa.Recruit.Vacancies.Jobs.QaDashboard;
using Esfa.Recruit.Vacancies.Jobs.VacancyStatus;
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

            services.AddRecruitStorageClient(configuration);

            // Add Jobs
            services.AddScoped<DomainEventsJob>();
            services.AddScoped<ApprenticeshipProgrammesJob>();
            services.AddScoped<VacancyStatusJob>();
            services.AddScoped<EmployerDashboardGeneratorJob>();
            services.AddScoped<PublishedVacanciesGeneratorJob>();
            services.AddScoped<BankHolidayJob>();
            services.AddScoped<QaDashboardJob>();
            services.AddScoped<NonLevyAccountBlockerJob>();

            // Domain Event Queue Handlers

            // Vacancy
            services.AddScoped<IDomainEventHandler<IEvent>, VacancyClonedDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<IEvent>, DraftVacancyUpdatedHandler>();
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

            //Candidate
            services.AddScoped<IDomainEventHandler<IEvent>, DeleteCandidateHandler>();
        }
    }
}
