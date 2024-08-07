using System;
using System.Collections.Generic;
using System.IO;
using Esfa.Recruit.Employer.Web.AppStart;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.Encoding;
using SFA.DAS.GovUK.Auth.AppStart;
using SFA.DAS.GovUK.Auth.Services;

namespace Esfa.Recruit.Employer.Web
{
    public partial class Startup
    {
        private readonly bool _isAuthEnabled = true;
        private IConfiguration Configuration { get; }
        private IWebHostEnvironment HostingEnvironment { get; }

        private readonly ILoggerFactory _loggerFactory;

        public Startup(IConfiguration config, IWebHostEnvironment env, ILoggerFactory loggerFactory)
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
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
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

            services.AddApplicationInsightsTelemetry(Configuration);

#if DEBUG
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
#endif
            
            services.AddTransient<ICustomClaims, EmployerAccountPostAuthenticationClaimsHandler>();
            services.AddAndConfigureGovUkAuthentication(Configuration, typeof(EmployerAccountPostAuthenticationClaimsHandler), "", "/services/SignIn-Stub");

            services.AddAuthorizationService();
            services.AddMaMenuConfiguration(RouteNames.Logout_Get, Configuration["ResourceEnvironmentName"]);
        


            services.AddDataProtection(Configuration, HostingEnvironment, applicationName: "das-employer");
            services.AddDasEncoding(Configuration);
        }
    }
}
