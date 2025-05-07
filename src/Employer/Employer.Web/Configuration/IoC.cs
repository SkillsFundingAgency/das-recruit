﻿using Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Filters;
using Esfa.Recruit.Employer.Web.Interfaces;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Models.AddLocation;
using Esfa.Recruit.Employer.Web.Models.Validators;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.TagHelpers;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.TrainingProvider;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage;
using Esfa.Recruit.Employer.Web.ViewModels.Validations;
using Esfa.Recruit.Shared.Web.Configuration;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.RuleTemplates;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Shared.Web.ViewModels.Validations.Fluent;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Ioc;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

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
            services.AddSingleton<ManageApprenticeshipsLinkHelper>();

            services.Configure<GoogleAnalyticsConfiguration>(configuration.GetSection("GoogleAnalytics"));
            services.Configure<ZenDeskConfiguration>(configuration.GetSection("ZenDesk"));

            services.AddFeatureToggle();
            services.AddFeatureManagement(configuration.GetSection("Features"));

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // Used by NLog to log out traceidentifier value.
            
            RegisterServiceDeps(services, configuration);

            RegisterFluentValidators(services);

            RegisterOrchestratorDeps(services);

            RegisterMapperDeps(services);

            RegisterFilterDeps(services);
        }

        private static void RegisterServiceDeps(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(new RecruitConfiguration(configuration.GetValue<string>("RecruitConfiguration:EmployerAccountId")));
            services.AddTransient<IGeocodeImageService>(_ => new GoogleMapsGeocodeImageService(configuration.GetValue<string>("RecruitConfiguration:GoogleMapsPrivateKey")));
            services.AddTransient<IReviewSummaryService, ReviewSummaryService>();
            services.AddTransient<ILegalEntityAgreementService, LegalEntityAgreementService>();
            services.AddTransient<AlertViewModelService>();
            services.AddTransient<IEmployerAlertsViewModelFactory, EmployerAlertsViewModelFactory>();
            services.AddTransient<IUtility, Utility>();
            services.AddTransient<IFieldReviewHelper, FieldReviewHelper>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IReviewFieldIndicatorService, ReviewFieldIndicatorService>();
            services.AddSingleton<IVacancyLocationService, VacancyLocationService>();
        }

        private static void RegisterFluentValidators(IServiceCollection services)
        {
            services.AddSingleton<IValidator<ApplicationReviewEditModel>, ApplicationReviewEditModelValidator>();
            services.AddSingleton<IValidator<WageEditModel>, WageEditModelValidator>();
            services.AddSingleton<IValidator<CompetitiveWageEditModel>, CompetitiveWageEditModelValidator>();
            services.AddSingleton<IValidator<ApplicationReviewStatusConfirmationEditModel>, ApplicationReviewStatusConfirmationEditModelValidator>();
            services.AddSingleton<IValidator<ApplicationReviewsToUnsuccessfulRequest>, ApplicationReviewsToUnsuccessfulRouteModelValidator>();
            services.AddSingleton<IValidator<SelectTrainingProviderEditModel>, SelectTrainingProviderEditModelValidator>();
            services.AddSingleton<IValidator<ConfirmTrainingProviderEditModel>, ConfirmTrainingProviderEditModelValidator>();
            services.AddSingleton<IValidator<ApplicationReviewsFeedbackViewModel>, ApplicationReviewsFeedbackModelValidator>();
            services.AddSingleton<IValidator<ApplicationReviewsToUnsuccessfulConfirmationViewModel>, ApplicationReviewsToUnsuccessfulConfirmationViewModelValidator>();
            services.AddSingleton<IValidator<AddLocationEditModel>, AddLocationEditModelValidator>();
            services.AddSingleton<IValidator<EnterLocationManuallyEditModel>, EnterLocationManuallyEditModelValidator>();
        }

        private static void RegisterOrchestratorDeps(IServiceCollection services)
        {
            services.AddTransient<AboutEmployerOrchestrator>();
            services.AddTransient<ApplicationProcessOrchestrator>();
            services.AddTransient<CloseVacancyOrchestrator>();
            services.AddTransient<ConsiderationsOrchestrator>();
            services.AddTransient<VacanciesOrchestrator>();
            services.AddTransient<EmployerOrchestrator>();
            services.AddTransient<EmployerNameOrchestrator>();
            services.AddTransient<LocationOrchestrator>();
            services.AddTransient<LegalEntityAgreementOrchestrator>();
            services.AddTransient<EmployerContactDetailsOrchestrator>();
            services.AddTransient<TitleOrchestrator>();
            services.AddTransient<NumberOfPositionsOrchestrator>();
            services.AddTransient<VacancyPreviewOrchestrator>();
            services.AddTransient<SubmittedOrchestrator>();
            services.AddTransient<EmployerVacancyOrchestrator>();
            services.AddTransient<TrainingProviderOrchestrator>();
            services.AddTransient<DeleteVacancyOrchestrator>();
            services.AddTransient<ShortDescriptionOrchestrator>();
            services.AddTransient<TrainingOrchestrator>();
            services.AddTransient<VacancyDescriptionOrchestrator>();
            services.AddTransient<VacancyWorkDescriptionOrchestrator>();
            services.AddTransient<VacancyHowWillTheApprenticeTrainOrchestrator>();
            services.AddTransient<IWageOrchestrator, WageOrchestrator>();
            services.AddTransient<CustomWageOrchestrator>();
            services.AddTransient<SkillsOrchestrator>();
            services.AddTransient<QualificationsOrchestrator>();
            services.AddTransient<VacancyManageOrchestrator>();
            services.AddTransient<VacancyViewOrchestrator>();
            services.AddTransient<IVacancyAnalyticsOrchestrator, VacancyAnalyticsOrchestrator>();
            services.AddTransient<IApplicationReviewOrchestrator, ApplicationReviewOrchestrator>();
            services.AddTransient<IApplicationReviewsOrchestrator, ApplicationReviewsOrchestrator>();
            services.AddTransient<EditVacancyDatesOrchestrator>();
            services.AddTransient<ManageNotificationsOrchestrator>();
            services.AddTransient<DatesOrchestrator>();
            services.AddTransient<VacanciesSearchSuggestionsOrchestrator>();
            services.AddTransient<DurationOrchestrator>();
            services.AddTransient<DashboardOrchestrator>();
            services.AddTransient<AlertsOrchestrator>();
            services.AddTransient<CloneVacancyOrchestrator>();
            services.AddTransient<VacancyTaskListOrchestrator>();
            services.AddTransient<IFutureProspectsOrchestrator, FutureProspectsOrchestrator>();
            services.AddTransient<IAdditionalQuestionsOrchestrator, AdditionalQuestionsOrchestrator>();
        }

        private static void RegisterMapperDeps(IServiceCollection services)
        {
            services.AddTransient<DisplayVacancyViewModelMapper>();
            services.AddTransient<ReviewFieldIndicatorMapper>();

            services.AddScoped<IRuleMessageTemplateRunner, RuleMessageTemplateRunner>();
        }

        private static void RegisterFilterDeps(IServiceCollection services)
        {
            services.AddScoped<GoogleAnalyticsFilter>();
            services.AddScoped<ZendeskApiFilter>();
        }

    }
}