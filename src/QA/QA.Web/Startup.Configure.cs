﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Configuration.Routing;
using Esfa.Recruit.Qa.Web.Security;
using Esfa.Recruit.Shared.Web.Middleware;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.DfESignIn.Auth.Configuration;

namespace Esfa.Recruit.Qa.Web
{
    public partial class Startup
    {
        public void Configure(ILogger<Startup> logger, IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime applicationLifetime)
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
                var rewriteOptions = new RewriteOptions()
                    .AddRedirectToHttps();

                app.UseRewriter(rewriteOptions);

                app.UseExceptionHandler(RoutePaths.ExceptionHandlingPath);
                app.UseMiddleware<RecruitExceptionHandlerMiddleware>(RoutePaths.ExceptionHandlingPath);

                app.UseHsts(hsts => hsts.MaxAge(365));
            }

            app.UseCsp(options => options
                .DefaultSources(s => s.Self())
                .StyleSources(s =>
                    s.Self()
                    //TinyMCE uses inline styles
                    .UnsafeInline()
                )
                .ScriptSources(s =>
                    s.Self()
                    .CustomSources("https://az416426.vo.msecnd.net/scripts/a/ai.0.js")
                )
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

            app.UseRedirectValidation(opts =>
            {
                opts.AllowSameHostRedirectsToHttps();
                opts.AllowedDestinations(GetAllowableDestinations(_dfEOidcConfig));
            }); //Register this earlier if there's middleware that might redirect.

            app.UseAuthentication();

            app.Use(async (context, next) => {
                if (context.Request.Path.Equals("/signout"))
                {
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    // Redirects
                    await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
                    {
                        RedirectUri = "/home"
                    });

                    return;
                }

                await next();
            });

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();

                endpoints.MapGet("/", context =>
                {
                    return Task.Run(() => context.Response.Redirect("/home"));
                });
            });

            //Registered after static files, to set headers for dynamic content.
            app.UseXfo(xfo => xfo.Deny());
            app.UseXDownloadOptions();
            app.UseXRobotsTag(options => options.NoIndex().NoFollow());

            app.UseNoCacheHttpHeaders(); // Affectively forces the browser to always request dynamic pages

        }

        private static string[] GetAllowableDestinations(DfEOidcConfiguration dfeConfig)
        {
            var destinations = new List<string>();

                        // add DfeSignIn base url to the whitelist/safe list. 
            if (!string.IsNullOrWhiteSpace(dfeConfig.BaseUrl))
                destinations.Add(dfeConfig.BaseUrl);
        
            return destinations.ToArray();
        }
    }
}
