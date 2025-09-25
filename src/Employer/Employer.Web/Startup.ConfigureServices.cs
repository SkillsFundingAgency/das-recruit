using System;
using System.IO;
using Esfa.Recruit.Employer.Web.AppStart;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.GovUK.Auth.AppStart;
using SFA.DAS.GovUK.Auth.Employer;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;

namespace Esfa.Recruit.Employer.Web
{
    public partial class Startup
    {
        private readonly bool _isAuthEnabled = true;
        private IConfiguration Configuration { get; }
        private IWebHostEnvironment HostingEnvironment { get; }

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<Startup> _logger;

        public Startup(IConfiguration config, IWebHostEnvironment env, ILoggerFactory loggerFactory, ILogger<Startup> logger)
        {
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

            Configuration =  configBuilder.Build();
            HostingEnvironment = env;
            _loggerFactory = loggerFactory;
            _logger = logger;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOpenTelemetryRegistration(Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]!);
            services.AddIoC(Configuration);

            // Routing has to come before adding Mvc
            services.AddRouting(opt =>
            {
                opt.AppendTrailingSlash = true;
            });

            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.ViewLocationFormats.Add("/Views/Part1/{1}/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Part2/{1}/{0}" + RazorViewEngine.ViewExtension);
            });

            services.AddHealthChecks();
            
            services.AddMvcService(HostingEnvironment, _isAuthEnabled, _loggerFactory, Configuration);

#if DEBUG
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
#endif
            
            services.AddAndConfigureGovUkAuthentication(Configuration, new AuthRedirects
            {
                SignedOutRedirectUrl = "",
                LocalStubLoginPath = "/services/SignIn-Stub" 
            },null, typeof(UserAccountService));

            services.AddAuthorizationService();
            services.AddMaMenuConfiguration(RouteNames.Logout_Get, Configuration["ResourceEnvironmentName"]);
        
            services.AddDataProtection(Configuration, HostingEnvironment, applicationName: "das-employer");
            services.AddDasEncoding(Configuration);

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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking infrastructure");
            }
        }
    }
}
