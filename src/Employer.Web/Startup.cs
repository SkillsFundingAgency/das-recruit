using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Employer.Domain.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NLog.Config;
using NLog.Web;

namespace Esfa.Recruit.Employer.Web
{
    public class Startup
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
            });            

            services.AddApplicationInsightsTelemetry(_configuration);

            ConfigureAuthentication(services);

            services.Configure<ExternalLinksConfiguration>(_configuration.GetSection("ExternalLinks"));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,  IOptions<ExternalLinksConfiguration> externalLinks)
        {
            app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                var rewriteOptions = new RewriteOptions()
                    .AddRedirectToHttps();

                app.UseRewriter(rewriteOptions);

                app.UseExceptionHandler(exApp =>
                {
                    exApp.Run(ctx => 
                    {
                        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        return Task.CompletedTask;
                    });
                });
            }
            
            //Registered before static files to always set header
            app.UseHsts(hsts => hsts.MaxAge(365));
            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(opts => opts.NoReferrer());

            if (_isAuthEnabled)
            {
                app.UseAuthentication();
            }

            app.UseStaticFiles();

            //Registered after static files, to set headers for dynamic content.
            app.UseXfo(xfo => xfo.Deny());
            app.UseRedirectValidation(opts => {
                opts.AllowSameHostRedirectsToHttps();
                opts.AllowedDestinations(GetAllowableDestinations(_authConfig, externalLinks.Value));
            }) ; //Register this earlier if there's middleware that might redirect.
            
            app.UseXDownloadOptions();
            app.UseXRobotsTag(options => options.NoIndex().NoFollow()); 

            //app.UseNoCacheHttpHeaders(); // Affectively forces the browser to always request dynamic pages

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

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

        private static string[] GetAllowableDestinations(AuthenticationConfiguration authConfig, ExternalLinksConfiguration linksConfig)
        {
            var destinations = new List<string>();
            
            if (!String.IsNullOrWhiteSpace(authConfig?.Authority))
                destinations.Add(authConfig.Authority);
            
            if (!String.IsNullOrWhiteSpace(linksConfig?.ManageApprenticeshipSiteUrl))
                destinations.Add(linksConfig?.ManageApprenticeshipSiteUrl);

            return destinations.ToArray();
        }

        private class AuthenticationConfiguration
        {
            public bool IsDevEnabled { get; set; } = false;
            public string Authority { get; set; }
            public string MetaDataAddress { get; set; }
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
        }
    }
}
