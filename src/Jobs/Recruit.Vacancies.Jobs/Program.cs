using System;
using System.Threading.Tasks;
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
        private static readonly string EnvironmentName;
        private static readonly bool IsDevelopment;

        static Program()
        {
            EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            IsDevelopment = EnvironmentName?.Equals("Development", StringComparison.CurrentCultureIgnoreCase) ?? false;
        }

        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .UseEnvironment("Development")
                .ConfigureWebJobs(b =>
                {
                    b.AddAzureStorageCoreServices()
                    .AddAzureStorage()
                    .AddTimers();
                })
                .ConfigureAppConfiguration(b =>
                {
                    b.AddJsonFile("appSettings.json", optional: false)
                    .AddJsonFile($"appSettings.{EnvironmentName}.json", true)
                    .AddEnvironmentVariables();

                    if (IsDevelopment)
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
                        options.MaxPollingInterval = System.TimeSpan.FromSeconds(10);
                    });

                    services.ConfigureJobServices(context.Configuration);
                })
                .UseConsoleLifetime();

            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}
