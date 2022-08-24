using System;
using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Configuration;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.TableStore;
using Esfa.Recruit.Vacancies.Client.Ioc;
using Esfa.Recruit.Vacancies.Jobs.NServiceBus;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;

namespace Esfa.Recruit.Vacancies.Jobs
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            ILogger logger = null;
            try
            {
                var host = CreateHostBuilder().Build();
                using (host)
                {
                    logger = ((ILoggerFactory)host.Services.GetService(typeof(ILoggerFactory)))
                        .CreateLogger(nameof(Program));

                    CheckInfrastructure(host.Services, logger);

                    await host.RunAsync();
                }
            }
            catch (Exception ex)
            {
                logger?.LogCritical(ex, "The Job has met with a horrible end!!");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        private static IHostBuilder CreateHostBuilder()
        {
            return
                new HostBuilder()
                    .ConfigureHostConfiguration(configHost =>  
                    {  
                        configHost.SetBasePath(Directory.GetCurrentDirectory());  
                        configHost.AddEnvironmentVariables();
#if DEBUG
                        configHost.AddJsonFile("appSettings.json", true)
                            .AddJsonFile($"appSettings.Development.json", true);               
#endif
                    })  
                    .ConfigureWebJobs(b =>
                    {
                        b.AddAzureStorageCoreServices()
                            .AddAzureStorage()
                            .AddTimers();
                    })
                    .ConfigureAppConfiguration((hostBuilderContext, configBuilder)=>
                    {
                        
                        configBuilder
                            .AddAzureTableStorage(
                                options =>
                                {
                                    options.ConfigurationKeys = hostBuilderContext.Configuration["ConfigNames"].Split(",");
                                    options.EnvironmentName = hostBuilderContext.Configuration["Environment"];
                                    options.StorageConnectionString = hostBuilderContext.Configuration["ConfigurationStorageConnectionString"];
                                    options.PreFixConfigurationKeys = false;
                                }
                        );
                    })
                    .ConfigureLogging((context, b) =>
                    {
                        b.SetMinimumLevel(LogLevel.Trace);
                        b.AddDebug();
                        b.AddConsole();
                        b.AddNLog();
                        b.ConfigureRecruitLogging();
                        string instrumentationKey = context.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
                        if (!string.IsNullOrEmpty(instrumentationKey))
                        {
                            b.AddApplicationInsights(o => o.InstrumentationKey = instrumentationKey);
                        }
                    })
                    .ConfigureServices((context, services) =>
                    {
                        services.AddOptions();
                        services.AddSingleton<IQueueProcessorFactory, CustomQueueProcessorFactory>();
                        services.Configure<QueuesOptions>(options =>
                        {
                            //maximum number of queue messages that are picked up simultaneously to be executed in parallel (default is 16)
                            options.BatchSize = 1;
                            //Maximum number of retries before a queue message is sent to a poison queue (default is 5)
                            options.MaxDequeueCount = 5;
                            //maximum wait time before polling again when a queue is empty (default is 1 minute).
                            options.MaxPollingInterval = TimeSpan.FromSeconds(10);
                        });

                        services.ConfigureJobServices(context.Configuration);

                        services.AddDasNServiceBus(context.Configuration);
                        services.AddApplicationInsightsTelemetryWorkerService(context.Configuration);
                    })
                    .UseConsoleLifetime();
        }

        private static void CheckInfrastructure(IServiceProvider serviceProvider, ILogger logger)
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
                logger?.LogError(ex, "Error checking infrastructure");
            }
        }
    }
}
