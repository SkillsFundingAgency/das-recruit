﻿using Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Filters;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.TrainingProvider;
using Esfa.Recruit.Shared.Web.Configuration;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.RuleTemplates;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Shared.Web.ViewModels.Validations.Fluent;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.FAA;
using Esfa.Recruit.Vacancies.Client.Ioc;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Esfa.Recruit.Employer.Web.Configuration
{
    public static class IoC
    {
        public static void AddIoC(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRecruitStorageClient(configuration);

            //Configuration
            services.Configure<ApplicationInsightsConfiguration>(configuration.GetSection("ApplicationInsights"));
            services.Configure<ExternalLinksConfiguration>(configuration.GetSection("ExternalLinks"));
            services.Configure<ManageApprenticeshipsRoutes>(configuration.GetSection("ManageApprenticeshipsRoutes"));
            services.AddSingleton<ManageApprenticeshipsLinkHelper>();

            services.Configure<AuthenticationConfiguration>(configuration.GetSection("Authentication"));
            services.Configure<GoogleAnalyticsConfiguration>(configuration.GetSection("GoogleAnalytics"));
            services.Configure<PostcodeAnywhereConfiguration>(configuration.GetSection("PostcodeAnywhere"));
            services.Configure<FaaConfiguration>(configuration.GetSection("FaaConfiguration"));

            services.AddFeatureToggle();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // Used by NLog to log out traceidentifier value.
            
            RegisterServiceDeps(services, configuration);

            RegisterFluentValidators(services);

            RegisterOrchestratorDeps(services);

            RegisterMapperDeps(services);

            RegisterFilterDeps(services);

            RegisterDynamicConfigurationDeps(services);
        }

        private static void RegisterServiceDeps(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IGeocodeImageService>(_ => new GoogleMapsGeocodeImageService(configuration.GetValue<string>("GoogleMapsPrivateKey")));
            services.AddTransient<IFaaService, FaaService>();
            services.AddTransient<IReviewSummaryService, ReviewSummaryService>();
            services.AddTransient<ILegalEntityAgreementService, LegalEntityAgreementService>();
            services.AddTransient<LevyDeclarationCookieWriter>();
            services.AddTransient<AlertViewModelService>();
        }

        private static void RegisterFluentValidators(IServiceCollection services)
        {
            services.AddSingleton<IValidator<ApplicationReviewEditModel>, ApplicationReviewEditModelValidator>();
            services.AddSingleton<IValidator<ApplicationReviewStatusConfirmationEditModel>, ApplicationReviewStatusConfirmationEditModelValidator>();
            services.AddSingleton<IValidator<SelectTrainingProviderEditModel>, SelectTrainingProviderEditModelValidator>();
            services.AddSingleton<IValidator<ConfirmTrainingProviderEditModel>, ConfirmTrainingProviderEditModelValidator>();            
        }

        private static void RegisterOrchestratorDeps(IServiceCollection services)
        {
            services.AddTransient<AboutEmployerOrchestrator>();
            services.AddTransient<ApplicationProcessOrchestrator>();
            services.AddTransient<CloseVacancyOrchestrator>();
            services.AddTransient<ConsiderationsOrchestrator>();
            services.AddTransient<DashboardOrchestrator>();
            services.AddTransient<EmployerOrchestrator>();
            services.AddTransient<EmployerNameOrchestrator>();
            services.AddTransient<LocationOrchestrator>();
            services.AddTransient<LegalEntityAgreementOrchestrator>();
            services.AddTransient<EmployerContactDetailsOrchestrator>();
            services.AddTransient<TitleOrchestrator>();
            services.AddTransient<NumberOfPositionsOrchestrator>();
            services.AddTransient<VacancyPreviewOrchestrator>();
            services.AddTransient<SubmittedOrchestrator>();
            services.AddTransient<TrainingProviderOrchestrator>();
            services.AddTransient<DeleteVacancyOrchestrator>();
            services.AddTransient<ShortDescriptionOrchestrator>();
            services.AddTransient<TrainingOrchestrator>();
            services.AddTransient<VacancyDescriptionOrchestrator>();
            services.AddTransient<WageOrchestrator>();
            services.AddTransient<SkillsOrchestrator>();
            services.AddTransient<QualificationsOrchestrator>();
            services.AddTransient<VacancyManageOrchestrator>();
            services.AddTransient<VacancyViewOrchestrator>();
            services.AddTransient<ApplicationReviewOrchestrator>();
            services.AddTransient<CreateVacancyOptionsOrchestrator>();
            services.AddTransient<EditVacancyDatesOrchestrator>();
            services.AddTransient<LevyDeclarationOrchestrator>();
            services.AddTransient<ManageNotificationsOrchestrator>();
            services.AddTransient<DatesOrchestrator>();
        }

        private static void RegisterMapperDeps(IServiceCollection services)
        {
            services.AddTransient<DisplayVacancyViewModelMapper>();
            services.AddTransient<ReviewFieldIndicatorMapper>();

            services.AddScoped<IRuleMessageTemplateRunner, RuleMessageTemplateRunner>();
        }

        private static void RegisterFilterDeps(IServiceCollection services)
        {
            services.AddScoped<PlannedOutageResultFilter>();
            services.AddScoped<CheckEmployerBlockedFilter>();
        }

        private static void RegisterDynamicConfigurationDeps(IServiceCollection services)
        {
            services.AddSingleton(x => 
            {
                var svc = x.GetService<IConfigurationReader>();
                return svc.GetAsync<EmployerRecruitSystemConfiguration>("EmployerRecruitSystem").Result;
            });
        }
    }
}