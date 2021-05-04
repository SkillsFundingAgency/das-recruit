using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Shared.Web.Configuration;
using Esfa.Recruit.Shared.Web.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Globalization;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Middleware;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.IdentityModel.Logging;

namespace Esfa.Recruit.Employer.Web
{
    public partial class Startup
    {
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IOptions<ExternalLinksConfiguration> externalLinks, IApplicationLifetime applicationLifetime, ILogger<Startup> logger)
        {
            var cultureInfo = new CultureInfo("en-GB");

            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            applicationLifetime.ApplicationStarted.Register(() => logger.LogInformation("Host fully started"));
            applicationLifetime.ApplicationStopping.Register(() => logger.LogInformation("Host shutting down...waiting to complete requests."));
            applicationLifetime.ApplicationStopped.Register(() => logger.LogInformation("Host fully stopped. All requests processed."));

            app.UseStatusCodePagesWithReExecute("/error/{0}");

            if (env.IsDevelopment())
            {
                var configuration = (TelemetryConfiguration)app.ApplicationServices.GetService(typeof(TelemetryConfiguration));
                configuration.DisableTelemetry = true;
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(RoutePaths.ExceptionHandlingPath);
                app.UseMiddleware<RecruitExceptionHandlerMiddleware>(RoutePaths.ExceptionHandlingPath);

                app.UseHsts(hsts => hsts.MaxAge(365));
            }

            if (env.IsDevelopment())
            {
                IdentityModelEventSource.ShowPII = true;
            }
            // Add Content Security Policy           
            app.UseCsp(options => options
                .DefaultSources(s =>
                {
                    s.Self()
                        .CustomSources(
                        "https://*.zdassets.com",
                        "https://*.zendesk.com",
                        "wss://*.zendesk.com",
                        "wss://*.zopim.com",
                        "https://*.rcrsv.io"
                        );
                    //s.UnsafeInline();
                })
                .StyleSources(s =>
                {
                    s.Self()
                    .CustomSources("https://www.googletagmanager.com/",
                                    "https://www.tagmanager.google.com/",
                                    "https://tagmanager.google.com/",
                                    "https://fonts.googleapis.com/",
                                    "https://*.zdassets.com",
                                    "https://*.zendesk.com",
                                    "wss://*.zendesk.com",
                                    "wss://*.zopim.com",
                                    "https://*.rcrsv.io"
                                    );

                    //Google tag manager uses inline styles when administering tags. This is done on PREPROD only
                    //TinyMCE uses inline styles
                    s.UnsafeInline();
                }
                )
                .ScriptSources(s =>
                {
                    s.Self()
                        .CustomSources("https://az416426.vo.msecnd.net/scripts/a/ai.0.js",
                                    "*.google-analytics.com",
                                    "*.googleapis.com",
                                    "*.googletagmanager.com/",                   
                                    "*.postcodeanywhere.co.uk/",
                                    "https://tagmanager.google.com",
                                    "https://www.tagmanager.google.com/",
                                    "https://www.google-analytics.com",
                                    "https://ssl.google-analytics.com",
                                    "https://*.zdassets.com",
                                    "https://*.zendesk.com",
                                    "wss://*.zendesk.com",
                                    "wss://*.zopim.com",
                                    "https://*.zopim.com",
                                    "https://*.rcrsv.io");

                    //Google tag manager uses inline scripts when administering tags. This is done on PREPROD only
                    if (env.IsEnvironment(EnvironmentNames.PREPROD))
                    {
                        s.UnsafeInline();
                        s.UnsafeEval();
                    }
                })
                .FontSources(s =>
                    s.Self()
                    .CustomSources("data:",
                                    "https://fonts.gstatic.com",
                                    "https://fonts.googleapis.com/",
                                    "https://assets-ukdoe.rcrsv.io/")
                )
                .ConnectSources(s =>
                    s.Self()
                    .CustomSources(
                        "https://*.zendesk.com",
                        "https://*.zdassets.com",
                        "https://dc.services.visualstudio.com",
                        "https://www.google-analytics.com",
                        "wss://*.zendesk.com",
                        "wss://*.zopim.com",
                        "https://*.rcrsv.io")
                )
                .ImageSources(s =>
                    s.Self()
                    .CustomSources("https://maps.googleapis.com",
                                    "*.google-analytics.com",
                                    "*.googletagmanager.com/",
                                    "https://ssl.gstatic.com",
                                    "https://www.gstatic.com/",                                    
                                    "https://*.zopim.io",
                                    "https://*.zdassets.com",
                                    "https://*.zendesk.com",
                                    "wss://*.zendesk.com",
                                    "wss://*.zopim.com",
                                    "data:")
                )
                .ReportUris(r => r.Uris("/ContentPolicyReport/Report")));



            //Registered before static files to always set header
            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(opts => opts.NoReferrer());
            app.UseXXssProtection(opts => opts.EnabledWithBlockMode());

            if (_isAuthEnabled)
            {
                app.UseAuthentication();
            }

            app.UseStaticFiles();

            //Registered after static files, to set headers for dynamic content.
            app.UseXfo(xfo => xfo.Deny());
            app.UseRedirectValidation(opts =>
            {
                opts.AllowSameHostRedirectsToHttps();
                opts.AllowedDestinations(GetAllowableDestinations(_authConfig, externalLinks.Value));
            }); //Register this earlier if there's middleware that might redirect.

            // Redirect requests to root to the MA site.
            app.UseRootRedirect(externalLinks.Value.ManageApprenticeshipSiteUrl);
            app.UseHttpsRedirection();

            app.UseXDownloadOptions();
            app.UseXRobotsTag(options => options.NoIndex().NoFollow());

            app.UseNoCacheHttpHeaders(); // Effectively forces the browser to always request dynamic pages

            app.UseMvc();
        }

        private static string[] GetAllowableDestinations(AuthenticationConfiguration authConfig, ExternalLinksConfiguration linksConfig)
        {
            var destinations = new List<string>();

            if (!string.IsNullOrWhiteSpace(authConfig?.Authority))
                destinations.Add(authConfig.Authority.Replace("identity", string.Empty));

            if (!string.IsNullOrWhiteSpace(linksConfig?.ManageApprenticeshipSiteUrl))
                destinations.Add(linksConfig?.ManageApprenticeshipSiteUrl);

            if (!string.IsNullOrWhiteSpace(linksConfig?.CommitmentsSiteUrl))
                destinations.Add(linksConfig?.CommitmentsSiteUrl);

            if (!string.IsNullOrWhiteSpace(linksConfig?.EmployerFavouritesUrl))
                destinations.Add(linksConfig.EmployerFavouritesUrl);

            return destinations.ToArray();
        }
    }
}
