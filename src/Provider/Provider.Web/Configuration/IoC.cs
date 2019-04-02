﻿using Esfa.Recruit.Provider.Web.Filters;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Configuration;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.RuleTemplates;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.FAA;
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
            services.Configure<PostcodeAnywhereConfiguration>(configuration.GetSection("PostcodeAnywhere"));
            services.Configure<FaaConfiguration>(configuration.GetSection("FaaConfiguration"));
            services.AddSingleton<ProviderApprenticeshipsLinkHelper>();

            services.AddFeatureToggle();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // Used by NLog to log out traceidentifier value.

            RegisterServiceDeps(services, configuration);

            RegisterOrchestratorDeps(services);

            RegisterMapperDeps(services);

            RegisterFilterDeps(services);

            RegisterDynamicConfigurationDeps(services);

            RegisterFluentValidators(services);           
        }        

        private static void RegisterServiceDeps(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IGeocodeImageService>(_ => new GoogleMapsGeocodeImageService(configuration.GetValue<string>("GoogleMapsPrivateKey")));
            services.AddTransient<IReviewSummaryService, ReviewSummaryService>();
            services.AddTransient<IFaaService, FaaService>();
        }

        private static void RegisterFluentValidators(IServiceCollection services)
        {
            services.AddSingleton<IValidator<ApplicationReviewEditModel>, ApplicationReviewEditModelValidator>();
            services.AddSingleton<IValidator<ApplicationReviewStatusConfirmationEditModel>, ApplicationReviewStatusConfirmationEditModelValidator>();
            services.AddSingleton<IValidator<ProviderApplicationsReportCreateEditModel>, ProviderApplicationsReportCreateEditModelValidator>();
        }

        private static void RegisterOrchestratorDeps(IServiceCollection services)
        {
            services.AddTransient<AboutEmployerOrchestrator>();
            services.AddTransient<ApplicationProcessOrchestrator>();
            services.AddTransient<ConsiderationsOrchestrator>();
            services.AddTransient<DashboardOrchestrator>();
            services.AddTransient<EmployerOrchestrator>();
            services.AddTransient<LocationOrchestrator>();
            services.AddTransient<ProviderContactDetailsOrchestrator>();
            services.AddTransient<QualificationsOrchestrator>();
            services.AddTransient<SearchResultPreviewOrchestrator>();
            services.AddTransient<ShortDescriptionOrchestrator>();
            services.AddTransient<SkillsOrchestrator>();
            services.AddTransient<SubmittedOrchestrator>();
            services.AddTransient<TitleOrchestrator>();
            services.AddTransient<TrainingOrchestrator>();
            services.AddTransient<VacancyDescriptionOrchestrator>();
            services.AddTransient<VacancyManageOrchestrator>();
            services.AddTransient<VacancyPreviewOrchestrator>();
            services.AddTransient<VacancyViewOrchestrator>();
            services.AddTransient<WageOrchestrator>();
            services.AddTransient<CloseVacancyOrchestrator>();
            services.AddTransient<EditVacancyDatesOrchestrator>();
            services.AddTransient<ApplicationReviewOrchestrator>();
            services.AddTransient<CloneVacancyOrchestrator>();
            services.AddTransient<DeleteVacancyOrchestrator>();
            services.AddTransient<ReportDashboardOrchestrator>();
            services.AddTransient<ProviderApplicationsReportOrchestrator>();
            services.AddTransient<ReportConfirmationOrchestrator>();
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
        }

        private static void RegisterDynamicConfigurationDeps(IServiceCollection services)
        {
            services.AddSingleton(x =>
                                                            {
                                                                var svc = x.GetService<IConfigurationReader>();
                                                                return svc.GetAsync<ProviderRecruitSystemConfiguration>("ProviderRecruitSystem").Result;
                                                            });
        }
    }
}