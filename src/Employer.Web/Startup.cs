using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Esfa.Recruit.Employer.Web
{
    public class Startup
    {
        public Startup(IConfiguration config, IHostingEnvironment env)
        {
            Configuration = config;
            HostingEnvironment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var authConfig = Configuration.GetSection("Authentication").Get<AuthenticationConfiguration>();
            
            services.AddMvc(opts =>
            {
                if (!authConfig.IsEnabled)
                {
                    opts.Filters.Add(new AllowAnonymousFilter());
                }
            });
              
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

                    options.Authority = authConfig.Authority;
                    options.MetadataAddress = authConfig.MetaDataAddress;
                    options.RequireHttpsMetadata = false;
                    options.ResponseType = "code";
                    options.ClientId = authConfig.ClientId;
                    options.ClientSecret = authConfig.ClientSecret;
                    options.Scope.Add("profile");
                    options.SaveTokens = true;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private class AuthenticationConfiguration
        {
            public bool IsEnabled { get; set; } = true;
            public string Authority { get; set; }
            public string MetaDataAddress { get; set; }
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
        }
    }
}
