using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Configuration;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.TableStore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;
using Esfa.Recruit.Vacancies.Client.Ioc;

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

                var host = CreateWebHostBuilder(args).Build();

                CheckInfrastructure(host.Services, logger);

                host.Run();
            }
            catch (Exception ex)
            {
                //NLog: catch setup errors
                logger.Fatal(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("https://localhost:5025")
                .UseNLog()
                .ConfigureLogging(b => b.ConfigureRecruitLogging());

        private static void CheckInfrastructure(IServiceProvider serviceProvider, NLog.ILogger logger)
        {
            try
            {
                var collectionChecker = (MongoDbCollectionChecker)serviceProvider.GetService(typeof(MongoDbCollectionChecker));
                collectionChecker.EnsureCollectionsExist();
                var storageTableChecker = (QueryStoreTableChecker)serviceProvider.GetService(typeof(QueryStoreTableChecker));
                storageTableChecker.EnsureQueryStoreTableExist();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error checking infrastructure");
            }
        }
    }
}
