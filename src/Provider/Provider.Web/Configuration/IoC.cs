﻿using Esfa.Recruit.Provider.Web.Filters;
using Esfa.Recruit.Provider.Web.Interfaces;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.Models.AddLocation;
using Esfa.Recruit.Provider.Web.Models.ApplicationReviews;
using Esfa.Recruit.Provider.Web.Models.Validators;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Configuration;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.RuleTemplates;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Shared.Web.ViewModels.Validations.Fluent;
using Esfa.Recruit.Provider.Web.Orchestrators.Reports;
using Esfa.Recruit.Provider.Web.ViewModels.Reports.ProviderApplicationsReport;
using Esfa.Recruit.Vacancies.Client.Ioc;
using FluentValidation;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Provider.Web.TagHelpers;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Esfa.Recruit.Provider.Web.ViewModels.Validations.Fluent;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Wage;
using Esfa.Recruit.Provider.Web.ViewModels.Validations;

namespace Esfa.Recruit.Provider.Web.Configuration
{
    public static class IoC
    {
        public static void AddIoC(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRecruitStorageClient(configuration);

            //Configuration
            services.Configure<ApplicationInsightsConfiguration>(configuration.GetSection("ApplicationInsights"));
            services.Configure<ExternalLinksConfiguration>(configuration.GetSection("ExternalLinks"));
            services.Configure<AuthenticationConfiguration>(configuration.GetSection("Authentication"));
            services.Configure<GoogleAnalyticsConfiguration>(configuration.GetSection("GoogleAnalytics"));
            services.Configure<ZenDeskConfiguration>(configuration.GetSection("ZenDesk"));
            services.AddSingleton<ProviderApprenticeshipsLinkHelper>();
            services.AddFeatureToggle();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // Used by NLog to log out traceidentifier value.

            RegisterServiceDeps(services, configuration);

            RegisterOrchestratorDeps(services);

            RegisterMapperDeps(services);

            RegisterFilterDeps(services);

            RegisterFluentValidators(services);
        }

        private static void RegisterServiceDeps(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IGeocodeImageService>(_ => new GoogleMapsGeocodeImageService(configuration.GetValue<string>("RecruitConfiguration:GoogleMapsPrivateKey")));
            services.AddTransient<IReviewSummaryService, ReviewSummaryService>();
            services.AddTransient<ILegalEntityAgreementService, LegalEntityAgreementService>();
            services.AddTransient<AlertViewModelService>();
            services.AddTransient<IProviderAlertsViewModelFactory, ProviderAlertsViewModelFactory>();
            services.AddTransient<ITrainingProviderAgreementService, TrainingProviderAgreementService>();
            services.AddTransient<IUtility, Utility>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddTransient<IFieldReviewHelper, FieldReviewHelper>();
            services.AddSingleton<IReviewFieldIndicatorService, ReviewFieldIndicatorService>();
            services.AddSingleton<IVacancyLocationService, VacancyLocationService>();
        }

        private static void RegisterFluentValidators(IServiceCollection services)
        {
            services.AddSingleton<IValidator<CompetitiveWageEditModel>, CompetitiveWageEditModelValidator>();
            services.AddSingleton<IValidator<ApplicationReviewEditModel>, ApplicationReviewEditModelValidator>();

            services.AddSingleton<IValidator<WageEditModel>, WageEditModelValidator>();

            services.AddSingleton<IValidator<ApplicationReviewFeedbackViewModel>, ApplicationReviewFeedbackModelValidator>();
            services.AddSingleton<IValidator<ApplicationReviewsToUnsuccessfulFeedbackViewModel>, ApplicationReviewsFeedbackModelValidator>();
            services.AddSingleton<IValidator<ApplicationReviewsToUnsuccessfulRequest>, ApplicationReviewsToUnsuccessfulModelValidator>();
            services.AddSingleton<IValidator<ApplicationReviewsToUnsuccessfulConfirmationViewModel>, ApplicationReviewsToUnsuccessfulConfirmationModelValidator>();

            services.AddSingleton<IValidator<ApplicationReviewStatusConfirmationEditModel>, ApplicationReviewStatusConfirmationEditModelValidator>();
            services.AddSingleton<IValidator<ProviderApplicationsReportCreateEditModel>, ProviderApplicationsReportCreateEditModelValidator>();
            services.AddSingleton<IValidator<ApplicationReviewsToShareRouteModel>, ApplicationReviewsToShareModelValidator>();
            services.AddSingleton<IValidator<AddLocationEditModel>, AddLocationEditModelValidator>();
            services.AddSingleton<IValidator<EnterLocationManuallyEditModel>, EnterLocationManuallyEditModelValidator>();
        }

        private static void RegisterOrchestratorDeps(IServiceCollection services)
        {
            services.AddTransient<AboutEmployerOrchestrator>();
            services.AddTransient<ApplicationProcessOrchestrator>();
            services.AddTransient<ConsiderationsOrchestrator>();
            services.AddTransient<VacanciesOrchestrator>();
            services.AddTransient<EmployerOrchestrator>();
            services.AddTransient<LegalEntityOrchestrator>();
            services.AddTransient<LegalEntityAndEmployerOrchestrator>();
            services.AddTransient<EmployerNameOrchestrator>();
            services.AddTransient<LegalEntityAgreementOrchestrator>();
            services.AddTransient<LocationOrchestrator>();
            services.AddTransient<ProviderContactDetailsOrchestrator>();
            services.AddTransient<QualificationsOrchestrator>();
            services.AddTransient<ShortDescriptionOrchestrator>();
            services.AddTransient<SkillsOrchestrator>();
            services.AddTransient<SubmittedOrchestrator>();
            services.AddTransient<ReviewedOrchestrator>();
            services.AddTransient<TitleOrchestrator>();
            services.AddTransient<NumberOfPositionsOrchestrator>();
            services.AddTransient<TrainingOrchestrator>();
            services.AddTransient<VacancyDescriptionOrchestrator>();
            services.AddTransient<IVacancyAnalyticsOrchestrator, VacancyAnalyticsOrchestrator>();
            services.AddTransient<VacancyManageOrchestrator>();
            services.AddTransient<VacancyPreviewOrchestrator>();
            services.AddTransient<VacancyViewOrchestrator>();
            services.AddTransient<IWageOrchestrator, WageOrchestrator>();
            services.AddTransient<ICustomWageOrchestrator,CustomWageOrchestrator>();
            services.AddTransient<CloseVacancyOrchestrator>();
            services.AddTransient<EditVacancyDatesOrchestrator>();
            services.AddTransient<IApplicationReviewOrchestrator, ApplicationReviewOrchestrator>();
            services.AddTransient<IApplicationReviewsOrchestrator, ApplicationReviewsOrchestrator>();
            services.AddTransient<CloneVacancyOrchestrator>();
            services.AddTransient<DeleteVacancyOrchestrator>();
            services.AddTransient<ReportDashboardOrchestrator>();
            services.AddTransient<ProviderApplicationsReportOrchestrator>();
            services.AddTransient<ReportConfirmationOrchestrator>();
            services.AddTransient<DashboardOrchestrator>();
            services.AddTransient<VacanciesSearchSuggestionsOrchestrator>();
            services.AddTransient<ManageNotificationsOrchestrator>();
            services.AddTransient<DatesOrchestrator>();
            services.AddTransient<AlertsOrchestrator>();
            services.AddTransient<ProviderAgreementOrchestrator>();
            services.AddTransient<DurationOrchestrator>();
            services.AddTransient<VacancyTaskListOrchestrator>();
            services.AddTransient<FutureProspectsOrchestrator>();
            services.AddTransient<IAdditionalQuestionsOrchestrator, AdditionalQuestionsOrchestrator>();
            services.AddTransient<VacancyWorkDescriptionOrchestrator>();
            services.AddTransient<VacancyHowWillTheApprenticeTrainOrchestrator>();
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