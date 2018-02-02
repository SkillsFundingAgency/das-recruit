using System.IdentityModel.Tokens.Jwt;
using Employer.Domain.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Esfa.Recruit.Employer.Web
{
    public partial class Startup
    {
        private readonly bool _isAuthEnabled = true;
        private IConfiguration _configuration { get; }
        private IHostingEnvironment _hostingEnvironment { get; }
        private AuthenticationConfiguration _authConfig { get; }

        public Startup(IConfiguration config, IHostingEnvironment env)
        {
            _configuration = config;
            _hostingEnvironment = env;
            _authConfig = _configuration.GetSection("Authentication").Get<AuthenticationConfiguration>();

            if (env.IsDevelopment()  && _authConfig.IsDevEnabled == false)
            {
                _isAuthEnabled = false;
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(opts =>
            {
                if (!_hostingEnvironment.IsDevelopment())
                {
                    opts.Filters.Add(new RequireHttpsAttribute());
                }

                if (!_isAuthEnabled)
                {
                    opts.Filters.Add(new AllowAnonymousFilter());
                }

                var policy = new AuthorizationPolicyBuilder()
                         .RequireAuthenticatedUser()
                         .Build();
                         
                opts.Filters.Add(new AuthorizeFilter(policy));
            });            

            services.AddApplicationInsightsTelemetry(_configuration);

            ConfigureAuthentication(services);

            services.Configure<ExternalLinksConfiguration>(_configuration.GetSection("ExternalLinks"));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

        private void ConfigureAuthentication(IServiceCollection services)
        {
            if (_isAuthEnabled)
            {
                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
                
                services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Cookies";
                    options.DefaultChallengeScheme = "oidc";
                })
                                .AddCookie("Cookies")
                                .AddOpenIdConnect("oidc", options =>
                                {
                                    options.SignInScheme = "Cookies";

                                    options.Authority = _authConfig.Authority;
                                    options.MetadataAddress = _authConfig.MetaDataAddress;
                                    options.RequireHttpsMetadata = false;
                                    options.ResponseType = "code";
                                    options.ClientId = _authConfig.ClientId;
                                    options.ClientSecret = _authConfig.ClientSecret;
                                    options.Scope.Add("profile");
                                });
            }
        }

        class AuthenticationConfiguration
        {
            public bool IsDevEnabled { get; set; } = false;
            public string Authority { get; set; }
            public string MetaDataAddress { get; set; }
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
        }
    }
}
