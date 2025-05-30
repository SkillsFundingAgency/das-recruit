﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Shared.Web.Configuration;
using Esfa.Recruit.Shared.Web.Middleware;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.Extensions.Hosting;
using SFA.DAS.DfESignIn.Auth.Configuration;

namespace Esfa.Recruit.Provider.Web
{
    public partial class Startup
    {
        private readonly DfEOidcConfiguration _dfEOidcConfig;

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<ExternalLinksConfiguration> externalLinks, IHostApplicationLifetime applicationLifetime, ILogger<Startup> logger, IConfiguration config)
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
            //AddCsp(app, env);

            //Registered before static files to always set header
            app.UseXContentTypeOptions();
            app.UseXXssProtection(opts => opts.EnabledWithBlockMode());

            app.UseAuthentication();

            app.UseStaticFiles();

            //Registered after static files, to set headers for dynamic content.
            app.UseXfo(xfo => xfo.Deny());
            app.UseRedirectValidation(opts =>
            {
                string sharedUiDashboardUrl = _configuration["ProviderSharedUIConfiguration:DashboardUrl"];
                opts.AllowSameHostRedirectsToHttps();
                opts.AllowedDestinations(GetAllowableDestinations(_authConfig, externalLinks.Value, _dfEOidcConfig, sharedUiDashboardUrl));
            }); //Register this earlier if there's middleware that might redirect.

            // Redirect requests to root of the provider site.
            app.UseRootRedirect(externalLinks.Value.ProviderApprenticeshipSiteUrl);

            app.UseXDownloadOptions();

            app.UseNoCacheHttpHeaders(); // Effectively forces the browser to always request dynamic pages
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(builder =>
            {
                builder.MapControllerRoute("default", RoutePaths.AccountRoutePath);
            });
        }

        private static void AddCsp(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCsp(options => options
                .DefaultSources(s =>
                {
                    s.Self()
                        .CustomSources(
                            "https://*.zdassets.com",
                            "https://*.zendesk.com",
                            "wss://*.zendesk.com",
                            "wss://*.zopim.com",
                            "https://*.rcrsv.io",
                            "https://*.signin.education.gov.uk"
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
                                "https://*.rcrsv.io",
                                "https://das-at-frnt-end.azureedge.net",
                                "https://das-test-frnt-end.azureedge.net",
                                "https://das-test2-frnt-end.azureedge.net",
                                "https://das-demo-frnt-end.azureedge.net",
                                "https://das-pp-frnt-end.azureedge.net",
                                "https://das-mo-frnt-end.azureedge.net",
                                "https://das-prd-frnt-end.azureedge.net",
                                "https://*.signin.education.gov.uk"
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
                            "https://tagmanager.google.com",
                            "https://www.tagmanager.google.com/",
                            "https://*.zdassets.com",
                            "https://*.zendesk.com",
                            "wss://*.zendesk.com",
                            "wss://*.zopim.com",
                            "https://*.zopim.com",
                            "https://*.rcrsv.io",
                            "https://das-at-frnt-end.azureedge.net",
                            "https://das-test-frnt-end.azureedge.net",
                            "https://das-test2-frnt-end.azureedge.net",
                            "https://das-demo-frnt-end.azureedge.net",
                            "https://das-pp-frnt-end.azureedge.net",
                            "https://das-mo-frnt-end.azureedge.net",
                            "https://das-prd-frnt-end.azureedge.net",
                            "https://*.signin.education.gov.uk");

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
                            "https://assets-ukdoe.rcrsv.io/",
                            "https://das-at-frnt-end.azureedge.net",
                            "https://das-test-frnt-end.azureedge.net",
                            "https://das-test2-frnt-end.azureedge.net",
                            "https://das-demo-frnt-end.azureedge.net",
                            "https://das-pp-frnt-end.azureedge.net",
                            "https://das-mo-frnt-end.azureedge.net",
                            "https://*.signin.education.gov.uk",
                            "https://das-prd-frnt-end.azureedge.net")
                )
                .ConnectSources(s =>
                    s.Self()
                        .CustomSources(
                            "https://*.zendesk.com",
                            "https://*.zdassets.com",
                            "https://dc.services.visualstudio.com",
                            "https://*.google-analytics.com",
                            "wss://*.zendesk.com",
                            "wss://*.zopim.com",
                            "https://*.signin.education.gov.uk",
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
                            "https://das-at-frnt-end.azureedge.net",
                            "https://das-test-frnt-end.azureedge.net",
                            "https://das-test2-frnt-end.azureedge.net",
                            "https://das-demo-frnt-end.azureedge.net",
                            "https://das-pp-frnt-end.azureedge.net",
                            "https://das-mo-frnt-end.azureedge.net",
                            "https://das-prd-frnt-end.azureedge.net",
                            "https://*.signin.education.gov.uk",
                            "data:")
                )
                .ReportUris(r => r.Uris("/ContentPolicyReport/Report")));
        }

        private static string[] GetAllowableDestinations(AuthenticationConfiguration authConfig, ExternalLinksConfiguration linksConfig, DfEOidcConfiguration dfEOidcConfiguration, string sharedUiDashboardUrl)
        {
            var destinations = new List<string>();

            if (!string.IsNullOrWhiteSpace(authConfig?.MetaDataAddress))
                destinations.Add(ExtractAuthHost(authConfig));

            if (!string.IsNullOrWhiteSpace(linksConfig?.ProviderApprenticeshipSiteUrl))
                destinations.Add(linksConfig.ProviderApprenticeshipSiteUrl);

            if (!string.IsNullOrWhiteSpace(linksConfig?.ProviderRecruitmentApiUrl))
                destinations.Add(linksConfig.ProviderRecruitmentApiUrl);
            
            if(!string.IsNullOrWhiteSpace(dfEOidcConfiguration.BaseUrl))
                destinations.Add(dfEOidcConfiguration.BaseUrl);

            if (!string.IsNullOrWhiteSpace(sharedUiDashboardUrl))
                destinations.Add(sharedUiDashboardUrl);

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
