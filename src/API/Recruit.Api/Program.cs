using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using NLog.Web;
using SFA.DAS.Recruit.Api.Helpers;

namespace SFA.DAS.Recruit.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var instance = HostingHelper.GetWebsiteInstanceId(); ;
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

            try
            {
                logger.Info($"Starting up host: ({instance})");
                var host = CreateWebHostBuilder(args).Build();

                host.Run();
            }
            catch (Exception ex)
            {
                //NLog: catch setup errors
                logger.Fatal(ex, "Stopped program because of exception");
                throw;
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                    .ConfigureKestrel(c => c.AddServerHeader = false)
                    .UseKestrel()
                    .UseNLog()
                    .UseUrls("https://localhost:5040")
                    .UseStartup<Startup>();
    }
}
