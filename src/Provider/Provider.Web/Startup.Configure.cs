using System;
using System.Collections.Generic;
using System.Globalization;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Shared.Web.Middleware;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Provider.Web
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

            // Add Content Security Policy
            app.UseCsp(options => options
                .DefaultSources(s => s.Self())
                .StyleSources(s =>
                    s.Self()
                    //TinyMCE uses inline styles
                    .UnsafeInline()
                )
                .ScriptSources(s =>
                    s.Self()
                    .CustomSources("https://az416426.vo.msecnd.net/scripts/a/ai.0.js",
                                    "https://www.google-analytics.com/analytics.js",
                                    "https://www.googletagmanager.com/",
                                    "https://www.tagmanager.google.com/",
                                    "https://services.postcodeanywhere.co.uk/")
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
                    .CustomSources("https://maps.googleapis.com",
                                    "https://www.google-analytics.com",
                                    "data:")
                 )
                .ReportUris(r => r.Uris("/ContentPolicyReport/Report")));

            //Registered before static files to always set header
            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(opts => opts.NoReferrer());
            app.UseXXssProtection(opts => opts.EnabledWithBlockMode());

            app.UseAuthentication();

            app.UseStaticFiles();

            //Registered after static files, to set headers for dynamic content.
            app.UseXfo(xfo => xfo.Deny());
            app.UseRedirectValidation(opts => {
                opts.AllowSameHostRedirectsToHttps();
                opts.AllowedDestinations(GetAllowableDestinations(_authConfig, externalLinks.Value));
            }); //Register this earlier if there's middleware that might redirect.

            // Redirect requests to root of the provider site.
            app.UseRootRedirect(externalLinks.Value.ProviderApprenticeshipSiteUrl);

            app.UseXDownloadOptions();
            app.UseXRobotsTag(options => options.NoIndex().NoFollow());

            app.UseNoCacheHttpHeaders(); // Effectively forces the browser to always request dynamic pages

            app.UseMvc(r => r.MapRoute("default", RoutePaths.AccountRoutePath));
        }

        private static string[] GetAllowableDestinations(AuthenticationConfiguration authConfig, ExternalLinksConfiguration linksConfig)
        {
            var destinations = new List<string>();

            if (!string.IsNullOrWhiteSpace(authConfig?.MetaDataAddress))
                destinations.Add(ExtractAuthHost(authConfig));

            if (!string.IsNullOrWhiteSpace(linksConfig?.ProviderApprenticeshipSiteUrl))
                destinations.Add(linksConfig.ProviderApprenticeshipSiteUrl);

            if (!string.IsNullOrWhiteSpace(linksConfig?.ProviderApprenticeshipSiteFeedbackUrl))
                destinations.Add(linksConfig.ProviderApprenticeshipSiteFeedbackUrl);

            if (!string.IsNullOrWhiteSpace(linksConfig?.CommitmentsSiteUrl))
                destinations.Add(linksConfig.CommitmentsSiteUrl);

            return destinations.ToArray();
        }

        private static string ExtractAuthHost(AuthenticationConfiguration authConfig)
        {
            var metaDataAddress = new Uri(authConfig.MetaDataAddress);
            var authHost = new UriBuilder(metaDataAddress.Scheme, metaDataAddress.Host).ToString();

            return authHost;
        }
    }
}
