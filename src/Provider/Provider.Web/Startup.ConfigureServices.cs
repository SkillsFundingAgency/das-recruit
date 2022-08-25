using System;
using System.IO;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Provider.Shared.UI.Startup;

namespace Esfa.Recruit.Provider.Web
{
    public partial class Startup
    {
        private readonly IConfigurationRoot _configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly AuthenticationConfiguration _authConfig;
        private readonly ILoggerFactory _loggerFactory;

        public Startup(IConfiguration config, IWebHostEnvironment env, ILoggerFactory loggerFactory)
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
            _loggerFactory = loggerFactory;
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

            services.AddMvcService(_hostingEnvironment, _loggerFactory);
            services.AddDataProtection(_configuration, _hostingEnvironment, applicationName: "das-provider-recruit-web");

            services.AddApplicationInsightsTelemetry(_configuration);

            //A service provider for resolving services configured in IoC
            var sp = services.BuildServiceProvider();

            services.AddProviderUiServiceRegistration(_configuration);

    #if DEBUG
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
    #endif

            services.AddAuthenticationService(_authConfig, sp.GetService<IRecruitVacancyClient>(), sp.GetService<IWebHostEnvironment>());
            services.AddAuthorizationService();            
        }
    }
}
