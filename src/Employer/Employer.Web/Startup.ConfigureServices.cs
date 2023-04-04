using System;
using System.IO;
using Esfa.Recruit.Employer.Web.AppStart;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.GovUK.Auth.AppStart;
using SFA.DAS.GovUK.Auth.Services;

namespace Esfa.Recruit.Employer.Web
{
    public partial class Startup
    {
        private readonly bool _isAuthEnabled = true;
        private IConfiguration Configuration { get; }
        private IWebHostEnvironment HostingEnvironment { get; }
        private AuthenticationConfiguration AuthConfig { get; }

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
            AuthConfig = Configuration.GetSection("Authentication").Get<AuthenticationConfiguration>();
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
            
            services.AddMvcService(HostingEnvironment, _isAuthEnabled, _loggerFactory);

            services.AddApplicationInsightsTelemetry(Configuration);

#if DEBUG
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
#endif

            if (Configuration["RecruitConfiguration:UseGovSignIn"] != null && Configuration["RecruitConfiguration:UseGovSignIn"]
                    .Equals("true", StringComparison.CurrentCultureIgnoreCase))
            {
                services.AddTransient<ICustomClaims, EmployerAccountPostAuthenticationClaimsHandler>();
                services.AddAndConfigureGovUkAuthentication(Configuration, $"{typeof(Startup).Assembly.GetName().Name}.Auth", typeof(EmployerAccountPostAuthenticationClaimsHandler));
                services.AddAuthorizationService();
            }

            else
            {
                if (_isAuthEnabled)
                {
                    services.AddAuthenticationService(AuthConfig);
                    services.AddAuthorizationService();
                }
            }


            services.AddDataProtection(Configuration, HostingEnvironment, applicationName: "das-employer-recruit-web");
        }
    }
}
