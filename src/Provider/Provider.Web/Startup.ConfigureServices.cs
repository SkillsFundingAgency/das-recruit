using System;
using System.Diagnostics;
using System.IO;
using Esfa.Recruit.Provider.Web.AppStart;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.DfESignIn.Auth.AppStart;
using SFA.DAS.DfESignIn.Auth.Configuration;
using SFA.DAS.DfESignIn.Auth.Enums;
using SFA.DAS.Provider.Shared.UI.Startup;

namespace Esfa.Recruit.Provider.Web
{
    public partial class Startup
    {
        private readonly IConfigurationRoot _configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly AuthenticationConfiguration _authConfig;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<Startup> _logger;

        public Startup(IConfiguration config, IWebHostEnvironment env, ILoggerFactory loggerFactory, ILogger<Startup> logger)
        {
            _hostingEnvironment = env;
            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(config)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();
                
#if DEBUG
            configBuilder
                .AddJsonFile("appsettings.json", optional:true)
                .AddJsonFile("appsettings.Development.json", optional: true);   
#endif
            
            configBuilder.AddAzureTableStorage(
                options => {
                    options.ConfigurationKeys = config["ConfigNames"].Split(",");
                    options.EnvironmentName = config["Environment"];
                    options.StorageConnectionString = config["ConfigurationStorageConnectionString"];
                    options.PreFixConfigurationKeys = false;
                }
            );

            _configuration =  configBuilder.Build();
            _authConfig = _configuration.GetSection("Authentication").Get<AuthenticationConfiguration>();
            
            _dfEOidcConfig = _configuration.GetSection("DfEOidcConfiguration").Get<DfEOidcConfiguration>(); // read the configuration from SFA.DAS.Provider.DfeSignIn
            _isDfESignInAllowed = _configuration.GetValue<bool>("UseDfeSignIn"); // read the UseDfeSignIn property from SFA.DAS.Recruit.QA configuration.
            _loggerFactory = loggerFactory;
            _logger = logger;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIoC(_configuration);

            // Routing has to come before adding Mvc
            services.AddRouting(opt =>
            {
                opt.LowercaseUrls = true;
                opt.AppendTrailingSlash = true;
            });

            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.ViewLocationFormats.Add("/Views/Part1/{1}/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Part2/{1}/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Reports/{1}/{0}" + RazorViewEngine.ViewExtension);
            });

            services.AddMvcService(_hostingEnvironment, _loggerFactory, _configuration);
            services.AddDataProtection(_configuration, _hostingEnvironment, applicationName: "das-provider");

            services.AddApplicationInsightsTelemetry(_configuration);
            services.AddOpenTelemetryRegistration(_configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]!);

            services.AddProviderUiServiceRegistration(_configuration);

    #if DEBUG
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
    #endif

            var serviceParameters = new ServiceParameters();
            bool useDfESignIn = _configuration["UseDfESignIn"] != null && _configuration["UseDfESignIn"].Equals("true", StringComparison.CurrentCultureIgnoreCase);
            if (useDfESignIn)
            {
                var providerType = ClientName.TraineeshipRoatp;
                if (serviceParameters.VacancyType == VacancyType.Apprenticeship)
                {
                    providerType = ClientName.ProviderRoatp;
                }
                services.AddAndConfigureDfESignInAuthentication(
                    _configuration,
                    "SFA.DAS.ProviderApprenticeshipService",
                    typeof(CustomServiceRole),
                    providerType,
                    "/signout",
                    "");    
            }
            else
            {
                services.AddAuthenticationService(_authConfig);    
            }
            
            services.AddAuthorizationService(useDfESignIn);
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
                var timer = Stopwatch.StartNew();
                _logger.LogInformation("Creating indexes");
                collectionChecker?.CreateIndexes().Wait();
                timer.Stop();
                _logger.LogInformation($"Finished creating indexes took:{timer.Elapsed.TotalSeconds}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking infrastructure");
            }
        }
    }
}
