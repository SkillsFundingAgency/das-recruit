﻿using System.Collections.Generic;
using Esfa.Recruit.Qa.Web.Configuration;
using Esfa.Recruit.Qa.Web.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Qa.Web
{
    public partial class Startup
    {
        public void Configure(ILogger<Startup> logger, IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
        {
            applicationLifetime.ApplicationStarted.Register(() => logger.LogInformation("Host fully started"));
            applicationLifetime.ApplicationStopping.Register(() => logger.LogInformation("Host shutting down...waiting to complete requests."));
            applicationLifetime.ApplicationStopped.Register(() => logger.LogInformation("Host fully stopped. All requests processed."));

            app.UseStatusCodePagesWithReExecute("/error/{0}");
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error/handle");
                app.UseHsts(hsts => hsts.MaxAge(365));
            }

            app.UseCsp(options => options
                .DefaultSources(s => s.Self())
                .StyleSources(s => 
                    s.Self()
                    .UnsafeInline()) // TODO: Should see if there's another option that having inline script - currently only need this for the built-in validation-summary which we are going to substitute with our own at some point.
                .ScriptSources(s => 
                    s.Self()
                    .CustomSources("https://az416426.vo.msecnd.net")
                    .UnsafeInline()
                ) // TODO: Look at moving AppInsights inline js code.
                .FontSources(s => 
                    s.Self()
                    .CustomSources("data:")
                )
                .ConnectSources(s => 
                    s.Self()
                    .CustomSources("https://dc.services.visualstudio.com")
                )
                .ImageSources(s => 
                    s.Self()
                    .CustomSources("https://maps.googleapis.com")
                 )
                .ReportUris(r => r.Uris("/ContentPolicyReport/Report")));

            //Registered before static files to always set header
            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(opts => opts.NoReferrer());
            app.UseXXssProtection(opts => opts.EnabledWithBlockMode());

            app.UseRedirectValidation(opts => {
                opts.AllowSameHostRedirectsToHttps();
                opts.AllowedDestinations(GetAllowableDestinations(_authenticationConfig, _externalLinks));
            }); //Register this earlier if there's middleware that might redirect.

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.Use(async (context, next) => {
                if (context.Request.Path.Equals("/signout"))
                {
                    // Redirects
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    await context.SignOutAsync(WsFederationDefaults.AuthenticationScheme, new AuthenticationProperties()
                    {
                        RedirectUri = "/"
                    });

                    return;
                }

                await next();
            });

            app.UseStaticFiles();

             //Registered after static files, to set headers for dynamic content.
            app.UseXfo(xfo => xfo.Deny());
            app.UseXDownloadOptions();
            app.UseXRobotsTag(options => options.NoIndex().NoFollow());

            app.UseNoCacheHttpHeaders(); // Affectively forces the browser to always request dynamic pages
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Dashboard}/{action=Index}/{id?}");
            });
        }

        private static string[] GetAllowableDestinations(AuthenticationConfiguration authConfig, ExternalLinksConfiguration linksConfig)
        {
            var destinations = new List<string>();
            
            if (!string.IsNullOrWhiteSpace(authConfig?.MetaDataAddress))
                destinations.Add(authConfig.MetaDataAddress);
            
            if (!string.IsNullOrWhiteSpace(linksConfig?.StaffIdamsUrl))
                destinations.Add(linksConfig.StaffIdamsUrl);

            return destinations.ToArray();
        }
    }
}
