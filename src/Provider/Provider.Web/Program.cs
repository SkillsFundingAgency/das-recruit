using System;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.TableStore;
using Esfa.Recruit.Vacancies.Client.Ioc;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NLog.Web;

namespace Esfa.Recruit.Provider.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
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
                NLog.LogManager.Shutdown();
            }
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(c => c.AddServerHeader = false)
                .UseStartup<Startup>()
                .UseUrls("https://localhost:5030")
                .UseNLog()
                .ConfigureLogging(b => b.ConfigureRecruitLogging());

        private static void CheckInfrastructure(IServiceProvider serviceProvider, NLog.ILogger logger)
        {
            try
            {
                var collectionChecker = serviceProvider.GetRequiredService<MongoDbCollectionChecker>();
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
