using System.Linq;
using Esfa.Recruit.Qa.Web.Configuration;
using Esfa.Recruit.Qa.Web.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

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
            // Routing has to come before adding Mvc
            services.AddRouting(opt =>
            {
                opt.AppendTrailingSlash = true;
            });

            services.AddMvcService();

            services.AddAntiforgery(
                options =>
                {
                    options.Cookie.Name = "_af";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.HeaderName = "X-XSRF-TOKEN";
                }
            );

            services.AddApplicationInsightsTelemetry(_configuration);
            services.AddAuthenticationService(_authenticationConfig);
            services.AddAuthorizationService(_authorizationConfig);
        }
    }
}
