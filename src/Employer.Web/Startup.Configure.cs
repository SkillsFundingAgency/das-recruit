using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Employer.Domain.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Employer.Web
{
    public partial class Startup
    {
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
            
             // Add Content Security Policy
            app.UseCsp(options => options
                .DefaultSources(s => s.Self())
                .ScriptSources(s => s.Self().CustomSources("https://az416426.vo.msecnd.net").UnsafeInline()) // TODO: Look at moving AppInsights inline js code.
                .ConnectSources(s => s.Self().CustomSources("https://dc.services.visualstudio.com"))
                .ReportUris(r => r.Uris("/ContentPolicyReport/Report")));

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

        private static string[] GetAllowableDestinations(AuthenticationConfiguration authConfig, ExternalLinksConfiguration linksConfig)
        {
            var destinations = new List<string>();
            
            if (!String.IsNullOrWhiteSpace(authConfig?.Authority))
                destinations.Add(authConfig.Authority);
            
            if (!String.IsNullOrWhiteSpace(linksConfig?.ManageApprenticeshipSiteUrl))
                destinations.Add(linksConfig?.ManageApprenticeshipSiteUrl);

            return destinations.ToArray();
        }
    }
}
