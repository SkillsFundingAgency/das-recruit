using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using NLog.Web;
using System;
using Esfa.Recruit.Vacancies.Client.Ioc;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Info("Starting up host");
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                //NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(c => c.AddServerHeader = false)
                .UseStartup<Startup>()
                .UseUrls("http://localhost:5020")
                .UseNLog()
                .ConfigureLogging(b => b.ConfigureRecruitLogging());
    }
}
