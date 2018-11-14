﻿using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Configuration;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Esfa.Recruit.Vacancies.Client.Ioc;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .UseEnvironment(RecruitEnvironment.EnvironmentName)
                .ConfigureWebJobs(b =>
                {
                    b.AddAzureStorageCoreServices()
                    .AddAzureStorage()
                    .AddTimers();
                })
                .ConfigureAppConfiguration(b =>
                {
                    b.AddJsonFile("appSettings.json", optional: false)
                    .AddJsonFile($"appSettings.{RecruitEnvironment.EnvironmentName}.json", true)
                    .AddEnvironmentVariables();

                    if (RecruitEnvironment.IsDevelopment)
                    {
                        b.AddUserSecrets<Program>();
                    }
                })
                .ConfigureLogging((context, b) =>
                {
                    b.SetMinimumLevel(LogLevel.Trace);
                    b.AddDebug();
                    b.AddConsole();
                    b.AddNLog();

                    // If this key exists in any config, use it to enable App Insights
                    string appInsightsKey = context.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
                    if (!string.IsNullOrEmpty(appInsightsKey))
                    {
                        b.AddApplicationInsights(o => o.InstrumentationKey = appInsightsKey);
                    }

                    b.ConfigureRecruitLogging();
                })
                .ConfigureServices((context, services) =>
                {
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
                })
                .UseConsoleLifetime();

            var host = builder.Build();
            using (host)
            {
                CheckInfrastructure(host.Services);

                await host.RunAsync();
            }
        }

        private static void CheckInfrastructure(IServiceProvider serviceProvider)
        {
            var collectionChecker = (MongoDbCollectionChecker)serviceProvider.GetService(typeof(MongoDbCollectionChecker));
            collectionChecker.EnsureCollectionsExist();
        }
    }
}
