using System;
using System.IO;
using Esfa.Recruit.Qa.Web.AppStart;
using Esfa.Recruit.Qa.Web.Configuration;
using Esfa.Recruit.Qa.Web.Mappings;
using Esfa.Recruit.Qa.Web.Orchestrators;
using Esfa.Recruit.Qa.Web.Orchestrators.Reports;
using Esfa.Recruit.Qa.Web.Security;
using Esfa.Recruit.QA.Web.Configuration;
using Esfa.Recruit.QA.Web.Filters;
using Esfa.Recruit.QA.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Configuration;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.RuleTemplates;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.TableStore;
using Esfa.Recruit.Vacancies.Client.Ioc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.DfESignIn.Auth.Configuration;

namespace Esfa.Recruit.Qa.Web
{
    public partial class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly AuthenticationConfiguration _authenticationConfig;
        private readonly AuthorizationConfiguration _legacyAuthorizationConfig;
        private readonly AuthorizationConfiguration _authorizationConfig;
        private readonly ExternalLinksConfiguration _externalLinks;
        private readonly DfEOidcConfiguration _dfEOidcConfig;
        private readonly bool _isDfESignInAllowed = false;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<Startup> _logger;

        public Startup(IConfiguration configuration, IWebHostEnvironment env, ILoggerFactory loggerFactory, ILogger<Startup> logger)
        {
            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();
                
#if DEBUG
            configBuilder
                .AddJsonFile("appsettings.json", optional:true)
                .AddJsonFile("appsettings.Development.json", optional: true);   
#endif   
            
            configBuilder.AddAzureTableStorage(
                options => {
                    options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                    options.EnvironmentName = configuration["Environment"];
                    options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                    options.PreFixConfigurationKeys = false;
                }
            );

            _configuration =  configBuilder.Build();
            _hostingEnvironment = env;
            _authenticationConfig = _configuration.GetSection("Authentication").Get<AuthenticationConfiguration>();
            _legacyAuthorizationConfig = _configuration.GetSection("LegacyAuthorization").Get<AuthorizationConfiguration>();
            _authorizationConfig = _configuration.GetSection("Authorization").Get<AuthorizationConfiguration>();
            _externalLinks = _configuration.GetSection("ExternalLinks").Get<ExternalLinksConfiguration>();
            _dfEOidcConfig = _configuration.GetSection("DfEOidcConfiguration").Get<DfEOidcConfiguration>(); // read the configuration from SFA.DAS.Provider.DfeSignIn
            _isDfESignInAllowed = _configuration.GetValue<bool>("UseDfESignIn"); // read the UseDfESignIn property from SFA.DAS.Recruit.QA configuration.
            _loggerFactory = loggerFactory;
            _logger = logger;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ApplicationInsightsConfiguration>(_configuration.GetSection("ApplicationInsights"));
            services.AddOpenTelemetryRegistration(_configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]!);

            // Routing has to come before adding Mvc
            services.AddRouting(opt =>
            {
                opt.AppendTrailingSlash = true;
            });

            services.AddMvcService(_loggerFactory);
            services.AddDataProtection(_configuration, _hostingEnvironment, applicationName: "das-qa-recruit-web");

            services.AddAntiforgery(
                options =>
                {
                    options.Cookie.Name = CookieNames.AntiForgeryCookie;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.HeaderName = "X-XSRF-TOKEN";
                }
            );

            services.AddAuthenticationService(_authenticationConfig, _configuration);
            services.AddAuthorizationService(_legacyAuthorizationConfig, _authorizationConfig);

            services.AddRecruitStorageClient(_configuration);
            services.AddScoped<DashboardOrchestrator>();
            services.AddScoped<ReviewOrchestrator>();
            services.AddScoped<ReportDashboardOrchestrator>();
            services.AddScoped<ApplicationsReportOrchestrator>();
            services.AddScoped<BlockedOrganisationsOrchestrator>();
            services.AddScoped<UnblockOrganisationOrchestrator>();
            services.AddScoped<ReportConfirmationOrchestrator>();
            services.AddScoped<WithdrawVacancyOrchestrator>();
            services.AddTransient<UserAuthorizationService>();

            services.AddTransient<IGeocodeImageService>(_ => new GoogleMapsGeocodeImageService(_configuration.GetValue<string>("RecruitConfiguration:GoogleMapsPrivateKey")));
            services.AddScoped<ReviewMapper>();
            services.AddTransient<IReviewSummaryService, ReviewSummaryService>();
            services.AddTransient<ReviewFieldIndicatorMapper>();

            services.AddScoped<IRuleMessageTemplateRunner, RuleMessageTemplateRunner>();

            services.AddScoped<PlannedOutageResultFilter>();

            services.AddSingleton(x =>
            {
                var svc = x.GetService<IConfigurationReader>();
                return svc.GetAsync<QaRecruitSystemConfiguration>("QaRecruitSystem").Result;
            });

            services.AddSingleton(new ServiceParameters());
            
            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.ViewLocationFormats.Add("/Views/Reports/{1}/{0}" + RazorViewEngine.ViewExtension);
            });
            
            services.AddFeatureToggle();
            services.AddDasEncoding(_configuration);

            CheckInfrastructure(services);
        }

        private void CheckInfrastructure(IServiceCollection services)
        {
            try
            {
                var serviceProvider = services.BuildServiceProvider();
                var collectionChecker = (MongoDbCollectionChecker)serviceProvider.GetService(typeof(MongoDbCollectionChecker));
                collectionChecker?.EnsureCollectionsExist();
                collectionChecker?.CreateIndexes().Wait();
                var storageTableChecker = (QueryStoreTableChecker)serviceProvider.GetService(typeof(QueryStoreTableChecker));
                storageTableChecker?.EnsureQueryStoreTableExist();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking infrastructure");
            }
        }
    }
}
