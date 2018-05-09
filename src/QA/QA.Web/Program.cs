using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Esfa.Recruit.Qa.Web
{
    public class Program
    {
       public static void Main(string[] args)
        {
            // NLog: setup the logger first to catch all errors
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Info("Starting up host");
                
                var certificate = BuildCertificate();
                var host = BuildWebHost(args, certificate);
                
                host.Run();
            }
            catch (Exception ex)
            {
                //NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        private static X509Certificate2 BuildCertificate()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("certificate.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"certificate.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
                .Build();

            var certificateSettings = config.GetSection("certificateSettings");
            string certificateFileName = certificateSettings.GetValue<string>("filename");
            string certificatePassword = certificateSettings.GetValue<string>("password");

            var certificate = new X509Certificate2(certificateFileName, certificatePassword);

            return certificate;
        }

        private static IWebHost BuildWebHost(string[] args, X509Certificate2 certificate) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(c =>
                {
                    c.AddServerHeader = false;
                    c.Listen(IPAddress.Loopback, 5025, listenOptions =>
                    {
                        listenOptions.UseHttps(certificate);
                    });
                })
                .UseStartup<Startup>()
                .UseUrls("https://localhost:5025")
                .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        var hostingEnvironment = hostingContext.HostingEnvironment;
                        if (hostingEnvironment.IsDevelopment())
                        {
                            config.AddUserSecrets<Startup>();
                        }
                    })
                .UseNLog()
                .Build();
    }
}
