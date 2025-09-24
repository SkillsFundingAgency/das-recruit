﻿using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Shared.Web.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Globalization;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;
using Microsoft.Extensions.Hosting;
using SFA.DAS.GovUK.Auth.Extensions;


namespace Esfa.Recruit.Employer.Web
{
    public partial class Startup
    {
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
            
            app.UseHealthChecks();
            
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
                                    "https://*.rcrsv.io",
                                    "https://das-at-frnt-end.azureedge.net", 
                                    "https://das-test-frnt-end.azureedge.net", 
                                    "https://das-test2-frnt-end.azureedge.net",
                                    "https://das-demo-frnt-end.azureedge.net", 
                                    "https://das-pp-frnt-end.azureedge.net",
                                    "https://das-mo-frnt-end.azureedge.net",
                                    "https://das-prd-frnt-end.azureedge.net"
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
                                    "https://das-prd-frnt-end.azureedge.net"
                                    );
                    //Google tag manager uses inline scripts when administering tags
                    s.UnsafeInline();
                    s.UnsafeEval();
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
                                    "https://das-prd-frnt-end.azureedge.net"
                                    )
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
                        "https://*.rcrsv.io")
                )
                .WorkerSources(s=>
                    s.Self().CustomSources("blob:"))
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
                                    "data:")
                )
                .ReportUris(r => r.Uris("/ContentPolicyReport/Report")));



            //Registered before static files to always set header
            app.UseXContentTypeOptions();
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
                opts.AllowedDestinations(GetAllowableDestinations(externalLinks.Value, config));
            }); //Register this earlier if there's middleware that might redirect.

            // Redirect requests to root to the MA site.
            app.UseRootRedirect(externalLinks.Value.ManageApprenticeshipSiteUrl);
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(builder =>
            {
                builder.MapControllerRoute("default", RoutePaths.AccountRoutePath);
            });
            app.UseXDownloadOptions();
            
            app.UseNoCacheHttpHeaders(); // Effectively forces the browser to always request dynamic pages

        }

        private static string[] GetAllowableDestinations(ExternalLinksConfiguration linksConfig, IConfiguration configuration)
        {
            var destinations = new List<string>();

            if (!string.IsNullOrWhiteSpace(linksConfig?.ManageApprenticeshipSiteUrl))
                destinations.Add(linksConfig?.ManageApprenticeshipSiteUrl);

            if (!string.IsNullOrWhiteSpace(linksConfig?.CommitmentsSiteUrl))
                destinations.Add(linksConfig?.CommitmentsSiteUrl);

            if (!string.IsNullOrWhiteSpace(linksConfig?.TrainingProviderPermissionUrl))
                destinations.Add(linksConfig.TrainingProviderPermissionUrl);

            if (!string.IsNullOrWhiteSpace(linksConfig?.EmployerRecruitmentApiUrl))
                destinations.Add(linksConfig.EmployerRecruitmentApiUrl);
            
            destinations.Add("https://oidc.integration.account.gov.uk");
            destinations.Add("https://oidc.account.gov.uk");
            destinations.Add("".GetSignedOutRedirectUrl(configuration["ResourceEnvironmentName"]));
            var stubSignInRedirectUrl = RedirectExtension.GetStubSignInRedirectUrl("",configuration["ResourceEnvironmentName"]);
            if (!string.IsNullOrEmpty(stubSignInRedirectUrl))
            {
                destinations.Add(stubSignInRedirectUrl);    
            }
            
            
            return destinations.ToArray();
        }
    }
}
