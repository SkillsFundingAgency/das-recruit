using Esfa.Recruit.Qa.Web.Configuration;
using Esfa.Recruit.Qa.Web.Mappings;
using Esfa.Recruit.Qa.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Esfa.Recruit.Qa.Web
{
    public partial class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly AuthenticationConfiguration _authenticationConfig;
        private readonly AuthorizationConfiguration _authorizationConfig;
        private readonly ExternalLinksConfiguration _externalLinks;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            _authenticationConfig = _configuration.GetSection("Authentication").Get<AuthenticationConfiguration>();
            _authorizationConfig = _configuration.GetSection("Authorization").Get<AuthorizationConfiguration>();
            _externalLinks = _configuration.GetSection("ExternalLinks").Get<ExternalLinksConfiguration>();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //A service provider for resolving services configured in IoC
            var sp = services.BuildServiceProvider();

            // Routing has to come before adding Mvc
            services.AddRouting(opt =>
            {
                opt.AppendTrailingSlash = true;
            });

            services.AddMvcService();

            services.AddAntiforgery(
                options =>
                {
                    options.Cookie.Name = "qa-x-csrf";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.HeaderName = "X-XSRF-TOKEN";
                }
            );

            services.AddApplicationInsightsTelemetry(_configuration);
            services.AddAuthenticationService(_authenticationConfig);
            services.AddAuthorizationService(_authorizationConfig);

            services.AddRecruitStorageClient(_configuration);
            services.AddScoped<DashboardOrchestrator>();
            services.AddScoped<ReviewOrchestrator>();

            services.AddTransient<IGeocodeImageService>(_ => new GoogleMapsGeocodeImageService(_configuration.GetValue<string>("GoogleMapsPrivateKey")));
            services.AddScoped<ReviewMapper>();
        }
    }
}
